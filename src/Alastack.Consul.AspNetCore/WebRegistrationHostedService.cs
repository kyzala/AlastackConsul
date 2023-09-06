using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.AspNetCore;

public class WebRegistrationHostedService : BackgroundService
{
    private readonly ConsulOptions _consulOptions;
    public readonly IServerAddressesFactory _serverAddressesFactory;

    public WebRegistrationHostedService(IOptions<ConsulOptions> options, IServerAddressesFactory serverAddressesFactory)
    {
        _consulOptions = options.Value;
        _serverAddressesFactory = serverAddressesFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int timeout = 5;
        int timestep = 0;
        while (!stoppingToken.IsCancellationRequested && !_serverAddressesFactory.Configuered && timestep < timeout)
        {
            await Task.Delay(1000, stoppingToken);
            timestep++;
        }
        if (!stoppingToken.IsCancellationRequested && _serverAddressesFactory.Configuered)
        {
            var addresses = _serverAddressesFactory.GetServerAddresses();

            foreach (var address in addresses)
            {
                Console.WriteLine(address);
            }
        }
    }
}
