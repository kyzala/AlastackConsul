using Alastack.Consul.Registration;
using System.Collections.ObjectModel;

namespace Alastack.Consul.AspNetCore;

internal sealed class KestrelServerAddressesProvider : IServerAddressesProvider
{
    private readonly ICollection<string> _addresses = new Collection<string>();

    public bool Configured { get; set; }

    public void AddServerAddress(string address)
    {
        _addresses.Add(address);
    }

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
