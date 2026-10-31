using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alastack.Consul.Registration;

internal sealed class ConsulRegistrationHostedService : BackgroundService
{
    private static readonly TimeSpan DeregistrationTimeout = TimeSpan.FromSeconds(5);

    private readonly IRegistrationService _registrationService;
    private readonly ILogger<ConsulRegistrationHostedService> _logger;

    public ConsulRegistrationHostedService(
        IRegistrationService registrationService,
        ILogger<ConsulRegistrationHostedService> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register on the background task without blocking host startup: the server listen addresses
        // pushed by the hosting layer only become available while startup proceeds.
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await _registrationService.RegisterAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host is stopping before the registration completed.
        }
        catch (Exception exception)
        {
            // Registration is best effort: a Consul outage must not stop the host.
            _logger.LogError(exception, "Failed to register the service instances with Consul.");
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
            // The shutdown token is usually already cancelled, so deregistration gets its own budget.
            using var timeout = new CancellationTokenSource(DeregistrationTimeout);
            try
            {
                // Deregistration is idempotent on the Consul side, invoke it unconditionally.
                await _registrationService.DeregisterAsync(timeout.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to deregister the service instances from Consul.");
            }
        }
    }
}
