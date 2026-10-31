using System.IO;
using System.Net;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Xml;

namespace Alastack.Extensions.Configuration.Consul;

/// <summary>
/// Loads a single Consul KV value and exposes it as configuration.
/// </summary>
public sealed class ConsulConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ConsulConfigurationFormat _format;
    private readonly IConsulClient _client;
    private readonly bool _ownsClient;
    private readonly object _gate = new();
    private readonly string _path;
    private readonly TimeSpan _pollingWaitTime;
    private readonly bool _optional;
    private readonly bool _reloadOnChange;
    private readonly Action<ConsulLoadExceptionContext>? _onLoadException;
    private CancellationTokenSource? _watchTokens;
    private ulong _lastIndex;
    private bool _missingNotified;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfigurationProvider"/> class.
    /// </summary>
    /// <param name="source">The configuration source.</param>
    /// <param name="format">The resolved format of the KV value.</param>
    public ConsulConfigurationProvider(ConsulConfigurationSource source, ConsulConfigurationFormat format)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Snapshot the settings: the source is mutable and must not be read again once the provider is built.
        _format = format;
        _path = source.Path;
        _pollingWaitTime = source.PollingWaitTime;
        _optional = source.Optional;
        _reloadOnChange = source.ReloadOnChange;
        _onLoadException = source.OnLoadException;
        _ownsClient = source.ClientFactory == null;
        _client = (source.ClientFactory ?? DefaultClientFactory)();
    }

    /// <inheritdoc />
    public override void Load()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            if (!HandleException(exception))
            {
                throw;
            }
        }

        // Start watching even when the initial load failed and the exception was ignored,
        // otherwise a Consul outage at startup would permanently disable reloading.
        StartWatch();
    }

    private static IConsulClient DefaultClientFactory() => new ConsulClient();

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var result = await RequestAsync(new QueryOptions(), cancellationToken).ConfigureAwait(false);
        if (result.Outcome == RequestOutcome.Loaded)
        {
            Apply(result.Pair!.Value!);
        }
        else
        {
            HandleMissing();
        }
    }

    private void HandleMissing()
    {
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!_optional)
        {
            // The exception is surfaced by Load, so the watch loop must not report it again.
            _missingNotified = true;
            throw new InvalidOperationException($"The Consul key '{_path}' was not found.");
        }
    }

    /// <summary>
    /// Handles a key that is missing while watching. Unlike the initial load, the watch loop keeps
    /// running: the key may be recreated, so a deletion must not disable reloading.
    /// </summary>
    private void HandleMissingWhileWatching()
    {
        if (_optional)
        {
            // Drop stale values so the application observes the deletion.
            if (Data.Count > 0)
            {
                Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                OnReload();
            }

            return;
        }

        if (!_missingNotified)
        {
            _missingNotified = true;
            HandleException(new InvalidOperationException($"The Consul key '{_path}' was not found."));
        }
    }

    private void Apply(byte[] value)
    {
        using var stream = new MemoryStream(value);
        Data = Parse(_format, stream);
        _missingNotified = false;
    }

    private async Task<RequestResult> RequestAsync(QueryOptions options, CancellationToken cancellationToken)
    {
        var result = await _client.KV.Get(_path, options, cancellationToken).ConfigureAwait(false);
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            return new RequestResult(RequestOutcome.Missing, null);
        }

        var statusCode = (int)result.StatusCode;
        if (statusCode is < 200 or >= 300)
        {
            // Anything but 404 means the lookup itself failed, so surface it through OnLoadException
            // instead of mistaking it for a missing key.
            throw new HttpRequestException(
                $"The Consul KV lookup for '{_path}' failed with {statusCode} ({result.StatusCode}).");
        }

        if (options.WaitIndex != 0 && result.LastIndex <= options.WaitIndex)
        {
            // The blocking query timed out without a change: Consul returns the same data
            // with the same index, so there is nothing to apply or reload.
            return new RequestResult(RequestOutcome.Unchanged, null);
        }

        if (result.LastIndex > 0)
        {
            _lastIndex = result.LastIndex;
        }

        return result.Response?.Value == null
            ? new RequestResult(RequestOutcome.Missing, null)
            : new RequestResult(RequestOutcome.Loaded, result.Response);
    }

    private void StartWatch()
    {
        if (!_reloadOnChange)
        {
            return;
        }

        lock (_gate)
        {
            if (_disposed || _watchTokens != null)
            {
                return;
            }

            var tokens = new CancellationTokenSource();
            _watchTokens = tokens;
            _ = Task.Run(() => WatchAsync(tokens.Token));
        }
    }

    private async Task WatchAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await RequestAsync(
                    new QueryOptions { WaitIndex = _lastIndex, WaitTime = _pollingWaitTime },
                    cancellationToken).ConfigureAwait(false);

                if (result.Outcome == RequestOutcome.Unchanged)
                {
                    continue;
                }

                if (result.Outcome == RequestOutcome.Missing)
                {
                    HandleMissingWhileWatching();

                    // Consul answers a blocking query for a missing key immediately, so wait
                    // before polling again instead of spinning on the network round trip.
                    await Task.Delay(_pollingWaitTime, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                Apply(result.Pair!.Value!);
                OnReload();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // Report the failure to the configured callback but keep watching: since this runs
                // on a background task the exception has no caller, and stopping the loop here would
                // silently disable reloading for the rest of the process lifetime.
                HandleException(exception);

                try
                {
                    // Back off before retrying so a persistent error does not turn into a tight loop.
                    await Task.Delay(_pollingWaitTime, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _watchTokens?.Cancel();
            _watchTokens?.Dispose();
            _watchTokens = null;
        }

        // Only dispose a client this provider created itself; a client supplied through
        // ConsulConfigurationSource.ClientFactory may be shared by other providers.
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    private bool HandleException(Exception exception)
    {
        var context = new ConsulLoadExceptionContext(exception);
        _onLoadException?.Invoke(context);
        return context.Ignore;
    }

    internal static IDictionary<string, string?> Parse(ConsulConfigurationFormat format, Stream stream)
    {
        var builder = new ConfigurationBuilder();
        switch (format)
        {
            case ConsulConfigurationFormat.Json:
                builder.AddJsonStream(stream);
                break;
            case ConsulConfigurationFormat.Ini:
                builder.AddIniStream(stream);
                break;
            case ConsulConfigurationFormat.Xml:
                builder.AddXmlStream(stream);
                break;
            default:
                throw new InvalidOperationException($"The configuration format '{format}' is not supported.");
        }

        return builder.Build()
            .AsEnumerable()
            .Where(pair => pair.Value != null)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    private enum RequestOutcome
    {
        /// <summary>The key exists and its value must be applied.</summary>
        Loaded,

        /// <summary>The blocking query timed out without a change.</summary>
        Unchanged,

        /// <summary>The key does not exist, or the response carried no value.</summary>
        Missing
    }

    private readonly record struct RequestResult(RequestOutcome Outcome, KVPair? Pair);
}
