using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

internal class DynamicRegistrationHostedService : BackgroundService
{
    private readonly ConsulRegistrationOptions _registrationOptions;
    private readonly IRegistrationService _registrationService;
    private readonly IEnumerable<IServerAddressesHandler> _serverAddressesHandlers;

    private bool _successed;


    public DynamicRegistrationHostedService(IOptions<ConsulRegistrationOptions> options, IRegistrationService registrationService, IEnumerable<IServerAddressesHandler> serverAddressesHandlers)
    {
        _registrationOptions = options.Value;
        _registrationService = registrationService;
        _serverAddressesHandlers = serverAddressesHandlers;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            foreach (var handler in _serverAddressesHandlers)
            {
                var addresses = await handler.GetServerAddresses(stoppingToken);
                foreach (var address in addresses)
                {
                    var addr = new Uri(address);
                    if (!_registrationOptions.Instances.Any(ins => ins.Address == addr))
                    {
                        var instance = new ServiceInstance
                        {
                            Address = addr
                        };
                        instance.Configure(_registrationOptions);
                        _registrationOptions.Instances.Add(instance);
                    }
                }
            }
            await _registrationService.ServiceRegister(stoppingToken);
            _successed = true;
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_successed)
        {
            await _registrationService.ServiceDeregister(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }

}
