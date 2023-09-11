using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Alastack.Consul.AspNetCore;

public class ServerAddressesFilter : IStartupFilter
{
    private readonly IServer _server;
    //private readonly IConfiguration _configuration;
    public readonly IServerAddressesHandler _serverAddressesHandler;

    public ServerAddressesFilter(IServer server, IEnumerable<IServerAddressesHandler> serverAddressesHandlers)
    {
        _server = server;
        //_configuration = configuration;
        var handler = serverAddressesHandlers.FirstOrDefault(handler => handler.GetType() == typeof(KestrelServerAddressesHandler));

        if (handler != null)
        {
            _serverAddressesHandler = handler;
        }
        else 
        {
            throw new ArgumentNullException($"{nameof(KestrelServerAddressesHandler)} not configuered.");
        }
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
        if (serverAddressesFeature?.Addresses != null) 
        {
            foreach (var address in serverAddressesFeature.Addresses) 
            {
                _serverAddressesHandler.AddServerAddress(address);
            }
        }
        _serverAddressesHandler.Configuered = true;

        return next;
    }
}