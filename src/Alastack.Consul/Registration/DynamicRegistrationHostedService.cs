using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace Alastack.Consul.Registration;

public class DynamicRegistrationHostedService : BackgroundService
{
    private readonly ConsulOptions _consulOptions;
    private readonly IRegistrationService _registrationService;
    private readonly IEnumerable<IServerAddressesHandler> _serverAddressesHandlers;

    private bool _successed;


    public DynamicRegistrationHostedService(IOptions<ConsulOptions> options, IRegistrationService registrationService, IEnumerable<IServerAddressesHandler> serverAddressesHandlers)
    {
        _consulOptions = options.Value;
        _registrationService = registrationService;
        _serverAddressesHandlers = serverAddressesHandlers;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        if (!stoppingToken.IsCancellationRequested)
        {
            _consulOptions.Registration!.Instances ??= new Collection<RegistrationInstance>();
            foreach (var handler in _serverAddressesHandlers)
            {
                var addresses = await handler.GetServerAddresses(stoppingToken);
                foreach (var address in addresses)
                {
                    var addr = new Uri(address);
                    if (!_consulOptions.Registration!.Instances.Any(ins => ins.Address == addr))
                    {
                        var instance = new RegistrationInstance 
                        { 
                            Address = addr
                        };
                        instance.Configure(_consulOptions.Registration);
                        _consulOptions.Registration!.Instances.Add(instance);
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
