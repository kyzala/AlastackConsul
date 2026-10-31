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
public sealed class ConsulConfigurationProvider : ConfigurationProvider
{
    private readonly ConsulConfigurationSource _source;
    private readonly ConsulConfigurationFormat _format;
    private readonly IConsulClient _client;
    private readonly object _gate = new();
    private CancellationTokenSource? _watchTokens;
    private ulong _lastIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfigurationProvider"/> class.
    /// </summary>
    /// <param name="source">The configuration source.</param>
    /// <param name="format">The resolved format of the KV value.</param>
    public ConsulConfigurationProvider(ConsulConfigurationSource source, ConsulConfigurationFormat format)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _format = format;
        _client = (source.ClientFactory ?? DefaultClientFactory)();
    }

    /// <inheritdoc />
    public override void Load()
    {
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
    }

    private static IConsulClient DefaultClientFactory() => new ConsulClient();

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var pair = await RequestAsync(new QueryOptions(), cancellationToken).ConfigureAwait(false);
        if (pair?.Value == null)
        {
            HandleMissing();
            return;
        }

        Apply(pair.Value);
        StartWatch();
    }

    private void HandleMissing()
    {
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!_source.Optional)
        {
            throw new InvalidOperationException($"The Consul key '{_source.Path}' was not found.");
        }
    }

    private void Apply(byte[] value)
    {
        using var stream = new MemoryStream(value);
        Data = Parse(_format, stream);
    }

    private async Task<KVPair?> RequestAsync(QueryOptions options, CancellationToken cancellationToken)
    {
        var result = await _client.KV.Get(_source.Path, options, cancellationToken).ConfigureAwait(false);
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (result.LastIndex > 0)
        {
            _lastIndex = result.LastIndex;
        }

        return result.Response;
    }

    private void StartWatch()
    {
        if (!_source.ReloadOnChange)
        {
            return;
        }

        lock (_gate)
        {
            if (_watchTokens != null)
            {
                return;
            }

            _watchTokens = new CancellationTokenSource();
            _ = Task.Run(() => WatchAsync(_watchTokens.Token));
        }
    }

    private async Task WatchAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var pair = await RequestAsync(
                    new QueryOptions { WaitIndex = _lastIndex, WaitTime = _source.PollingWaitTime },
                    cancellationToken).ConfigureAwait(false);

                if (pair?.Value == null)
                {
                    continue;
                }

                Apply(pair.Value);
                OnReload();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                if (!HandleException(exception))
                {
                    return;
                }
            }
        }
    }

    private bool HandleException(Exception exception)
    {
        var context = new ConsulLoadExceptionContext(exception);
        _source.OnLoadException?.Invoke(context);
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
}
