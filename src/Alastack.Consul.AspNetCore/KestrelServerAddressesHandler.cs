using Alastack.Consul.Registration;
using System.Collections.ObjectModel;

namespace Alastack.Consul.AspNetCore;

public class KestrelServerAddressesHandler : IServerAddressesHandler
{        
    private readonly ICollection<string> _addresses = new Collection<string>();

    public bool Configuered { get; set; }


    public KestrelServerAddressesHandler()
    {        
    }

    public void AddServerAddress(string address) 
    {
        _addresses.Add(address);
    }

    public async Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken)
    {
        if (Configuered) 
        {
            return _addresses;
        }

        int timestep = 0;
        while (!cancellationToken.IsCancellationRequested && !Configuered && timestep < Defaults.ServerAddressTimeout)
        {
            await Task.Delay(100, cancellationToken);
            timestep += 100;
        }
        if (Configuered)
        {
            return _addresses;
        }
        
        throw new ArgumentNullException("Server addresses not configuered.");
        
    }
}
