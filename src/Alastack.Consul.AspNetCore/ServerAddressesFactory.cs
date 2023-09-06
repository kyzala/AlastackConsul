using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace Alastack.Consul.AspNetCore;

public class ServerAddressesFactory : IServerAddressesFactory
{        
    private readonly IConfiguration _configuration;
    private readonly ConsulOptions _consulOptions;
    private readonly ICollection<string> _addresses = new Collection<string>();

    public bool Configuered { get; set; }

    public ServerAddressesFactory(IOptions<ConsulOptions> options, IConfiguration configuration)
    {
        _consulOptions = options.Value;
        _configuration = configuration;
    }

    public void AddServerAddress(string address) 
    {
        _addresses.Add(address);
    }

    public ICollection<string> GetServerAddresses()
    {
        if (Configuered) 
        {
            return _addresses;
        }
        throw new ArgumentNullException("Server addresses not configuered.");
        
    }
}
