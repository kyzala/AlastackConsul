using Microsoft.Extensions.Hosting;

namespace Alastack.Consul.Registration;

internal sealed class ConsulRegistrationHostedService : IHostedService
{
    private readonly IRegistrationService _registrationService;

    public ConsulRegistrationHostedService(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _registrationService.RegisterAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Deregistration is idempotent on the Consul side, invoke it unconditionally.
        return _registrationService.DeregisterAsync(cancellationToken);
    }
}
