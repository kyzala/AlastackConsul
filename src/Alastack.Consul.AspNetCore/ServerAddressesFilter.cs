using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Alastack.Consul.AspNetCore;

internal class ServerAddressesFilter : IStartupFilter
{
    private readonly IServer _server;
    private readonly DefaultServerAddressesProvider _addressesProvider;

    public ServerAddressesFilter(IServer server, DefaultServerAddressesProvider addressesProvider)
    {
        _server = server;
        _addressesProvider = addressesProvider;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
        if (serverAddressesFeature?.Addresses != null)
        {
            foreach (var address in serverAddressesFeature.Addresses)
            {
                _addressesProvider.AddServerAddress(address);
            }
        }
        _addressesProvider.Configured = true;

        return next;
    }
}
