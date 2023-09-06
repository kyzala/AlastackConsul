using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Alastack.Consul.AspNetCore;

public class ServerAddressesFilter : IStartupFilter
{
    private readonly IServer _server;
    private readonly IConfiguration _configuration;
    public readonly IServerAddressesFactory _serverAddressesFactory;

    public ServerAddressesFilter(IServer server, IConfiguration configuration, IServerAddressesFactory serverAddressesFactory)
    {
        _server = server;
        _configuration = configuration;
        _serverAddressesFactory = serverAddressesFactory;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
        if (serverAddressesFeature?.Addresses != null) 
        {
            foreach (var address in serverAddressesFeature.Addresses) 
            {
                _serverAddressesFactory.AddServerAddress(address);
            }
        }
        _serverAddressesFactory.Configuered = true;

        return next;
    }
}