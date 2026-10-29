using Alastack.Consul.Registration;
using System.Collections.ObjectModel;

namespace Alastack.Consul.AspNetCore;

public class KestrelServerAddressesHandler : IServerAddressesHandler
{        
    private readonly ICollection<string> _addresses = new Collection<string>();

    public bool Configured { get; set; }


    public KestrelServerAddressesHandler()
    {        
    }

    public void AddServerAddress(string address) 
    {
        _addresses.Add(address);
    }

    public async Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken)
    {
        if (Configured) 
        {
            return _addresses;
        }

        int timestep = 0;
        while (!cancellationToken.IsCancellationRequested && !Configured && timestep < ServerAddressesDefaults.ServerAddressTimeout)
        {
            await Task.Delay(100, cancellationToken);
            timestep += 100;
        }
        if (Configured)
        {
            return _addresses;
        }
        
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        throw new TimeoutException("Server addresses were not configured within the timeout.");
        
    }
}
