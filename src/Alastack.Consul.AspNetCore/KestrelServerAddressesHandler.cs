using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace Alastack.Consul.AspNetCore;

public class KestrelServerAddressesHandler : IServerAddressesHandler
{        
    private readonly IConfiguration _configuration;
    private readonly ConsulOptions _consulOptions;
    private readonly ICollection<string> _addresses = new Collection<string>();
    private IServerAddressesCollector _serverAddressesCollector;

    public bool Configuered { get; set; }


    public KestrelServerAddressesHandler(IOptions<ConsulOptions> options, IConfiguration configuration, IServerAddressesCollector serverAddressesCollector)
    {
        _consulOptions = options.Value;
        _configuration = configuration;
        _serverAddressesCollector = serverAddressesCollector;
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
