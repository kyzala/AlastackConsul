using System.Collections.Concurrent;
using System.Text;
using Consul;

namespace Alastack.Extensions.Configuration.Consul;

/// <summary>
/// Creates the <see cref="IConsulClient"/> instances used to query Consul.
/// </summary>
public interface IConsulClientFactory
{
    /// <summary>
    /// Gets a client for the specified settings. Implementations may return a shared instance,
    /// so callers must not dispose the returned client.
    /// </summary>
    /// <param name="settings">The settings that identify the client.</param>
    /// <returns>The <see cref="IConsulClient"/>.</returns>
    IConsulClient GetClient(ConsulClientSettings settings);
}

/// <summary>
/// The settings that identify a Consul client. Instances with the same values share a client.
/// </summary>
public sealed record ConsulClientSettings
{
    /// <summary>
    /// Gets the Consul agent address. Defaults to the Consul client default, <c>http://localhost:8500</c>.
    /// </summary>
    public Uri Address { get; init; } = new("http://localhost:8500");

    /// <summary>
    /// Gets the datacenter to use. When not set, the agent default is used.
    /// </summary>
    public string? Datacenter { get; init; }

    /// <summary>
    /// Gets the ACL token. When not set, the agent default is used.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// Gets the maximum time a blocking query waits for changes. When not set, the agent default is used.
    /// </summary>
    public TimeSpan? WaitTime { get; init; }

    /// <summary>
    /// Gets the maximum time to wait for a Consul request to complete.
    /// When not set, the Consul client default is used.
    /// </summary>
    public TimeSpan? HttpClientTimeout { get; init; }

    // A sealed record synthesizes a private PrintMembers, so this member must be private as well.
    private bool PrintMembers(StringBuilder builder)
    {
        // Never print the token: settings may end up in logs through record ToString.
        builder.Append($"Address = {Address}, Datacenter = {Datacenter}, WaitTime = {WaitTime}, HttpClientTimeout = {HttpClientTimeout}");
        return true;
    }
}

/// <summary>
/// The default <see cref="IConsulClientFactory"/>: one shared client per distinct <see cref="ConsulClientSettings"/>.
/// </summary>
public sealed class ConsulClientFactory : IConsulClientFactory, IDisposable
{
    /// <summary>
    /// Gets the process wide factory used when no factory is supplied.
    /// It is not disposed and its clients live for the lifetime of the process.
    /// </summary>
    public static ConsulClientFactory Default { get; } = new();

    private readonly ConcurrentDictionary<ConsulClientSettings, Lazy<IConsulClient>> _clients = new();
    private volatile bool _disposed;

    /// <inheritdoc />
    public IConsulClient GetClient(ConsulClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConsulClientFactory));
        }

        return _clients.GetOrAdd(
            settings,
            static key => new Lazy<IConsulClient>(() => CreateClient(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var client in _clients.Values)
        {
            if (client.IsValueCreated)
            {
                client.Value.Dispose();
            }
        }

        _clients.Clear();
    }

    private static IConsulClient CreateClient(ConsulClientSettings settings)
    {
        void Configure(ConsulClientConfiguration config)
        {
            config.Address = settings.Address;

            // Only override the Consul client defaults when the setting was actually configured.
            if (!string.IsNullOrWhiteSpace(settings.Datacenter))
            {
                config.Datacenter = settings.Datacenter;
            }
            if (!string.IsNullOrWhiteSpace(settings.Token))
            {
                config.Token = settings.Token;
            }
            if (settings.WaitTime.HasValue)
            {
                config.WaitTime = settings.WaitTime;
            }
        }

        if (!settings.HttpClientTimeout.HasValue)
        {
            return new ConsulClient(Configure);
        }

        // ConsulClientConfiguration has no timeout setting, so the timeout is applied to the HttpClient.
        return new ConsulClient(
            Configure,
            httpClient => httpClient.Timeout = settings.HttpClientTimeout.Value);
    }
}
