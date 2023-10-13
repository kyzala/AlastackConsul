using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

/// <summary>
/// Hosted servie for the service registration.
/// </summary>
public class DefaultRegistrationHostedService : IHostedService
{
    private readonly ConsulOptions _consulOptions;
    private readonly IRegistrationService _registrationService;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultRegistrationHostedService"/>.
    /// </summary>
    /// <param name="options"><see cref="ConsulOptions"/></param>    
    /// <param name="registrationService"><see cref="IRegistrationService"/></param>
    public DefaultRegistrationHostedService(IOptions<ConsulOptions> options, IRegistrationService registrationService)
    {
        //if (options.Value.Registration == null) 
        //{
        //    throw new ArgumentNullException(nameof(IOptions<ConsulOptions>.Value.Registration));
        //}
        _consulOptions = options.Value;
        _registrationService = registrationService;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _registrationService.ServiceRegister(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _registrationService.ServiceDeregister(cancellationToken);
    }

}
