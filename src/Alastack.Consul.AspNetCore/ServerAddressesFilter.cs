using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Alastack.Consul.AspNetCore;

internal class ServerAddressesFilter : IStartupFilter
{
    private readonly IServer _server;
    public readonly IServerAddressesHandler _serverAddressesHandler;

    public ServerAddressesFilter(IServer server, IEnumerable<IServerAddressesHandler> serverAddressesHandlers)
    {
        _server = server;
        var handler = serverAddressesHandlers.FirstOrDefault(handler => handler.GetType() == typeof(KestrelServerAddressesHandler));

        if (handler != null)
        {
            _serverAddressesHandler = handler;
        }
        else 
        {
            throw new InvalidOperationException($"{nameof(KestrelServerAddressesHandler)} not configured.");
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
        _serverAddressesHandler.Configured = true;

        return next;
    }
}