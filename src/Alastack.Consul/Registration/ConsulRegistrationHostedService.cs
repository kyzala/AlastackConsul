using Microsoft.Extensions.Hosting;

namespace Alastack.Consul.Registration;

internal sealed class ConsulRegistrationHostedService : BackgroundService
{
    private readonly IRegistrationService _registrationService;

    public ConsulRegistrationHostedService(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register on the background task without blocking host startup: the server listen addresses
        // pushed by the hosting layer only become available while startup proceeds.
        if (!stoppingToken.IsCancellationRequested)
        {
            await _registrationService.RegisterAsync(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Wait for the registration to complete before deregistering.
            await base.StopAsync(cancellationToken);
        }
        finally
        {
            // Deregistration is idempotent on the Consul side, invoke it unconditionally.
            await _registrationService.DeregisterAsync(cancellationToken);
        }
    }
}
