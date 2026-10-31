using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace Alastack.Consul.AspNetCore;

/// <summary>
/// Pushes the server listen addresses into the <see cref="DefaultServerAddressesProvider"/> once the host has started.
/// The addresses are only populated by the server while it starts, so reading them any earlier yields an empty set.
/// </summary>
internal sealed class ServerAddressesInitializer : IHostedService
{
    private readonly IServer _server;
    private readonly DefaultServerAddressesProvider _addressesProvider;
    private readonly IHostApplicationLifetime _lifetime;

    public ServerAddressesInitializer(
        IServer server,
        DefaultServerAddressesProvider addressesProvider,
        IHostApplicationLifetime lifetime)
    {
        _server = server;
        _addressesProvider = addressesProvider;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(
            () =>
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
            });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
