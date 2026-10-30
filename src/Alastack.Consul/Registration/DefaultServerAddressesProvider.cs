using System.Collections.ObjectModel;

namespace Alastack.Consul.Registration;

/// <summary>
/// The default <see cref="IServerAddressesProvider"/> that buffers the server listen addresses pushed by the hosting layer.
/// </summary>
public sealed class DefaultServerAddressesProvider : IServerAddressesProvider
{
    private readonly ICollection<string> _addresses = new Collection<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the hosting layer has finished pushing server addresses.
    /// </summary>
    public bool Configured { get; set; }

    /// <summary>
    /// Adds a server listen address.
    /// </summary>
    /// <param name="address">The server listen address.</param>
    public void AddServerAddress(string address)
    {
        _addresses.Add(address);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetServerAddressesAsync(CancellationToken cancellationToken)
    {
        int elapsed = 0;
        while (!Configured && !cancellationToken.IsCancellationRequested && elapsed < ServerAddressesDefaults.ServerAddressesTimeout)
        {
            await Task.Delay(100, cancellationToken);
            elapsed += 100;
        }

        if (Configured)
        {
            return _addresses.ToArray();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        throw new TimeoutException("Server addresses were not configured within the timeout.");
    }
}
