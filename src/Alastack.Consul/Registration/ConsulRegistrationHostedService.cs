using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

internal sealed class ConsulRegistrationHostedService : BackgroundService
{
    private static readonly TimeSpan DeregistrationTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(5);
    internal const int MaxRegistrationAttempts = 3;

    private readonly IRegistrationService _registrationService;
    private readonly ILogger<ConsulRegistrationHostedService> _logger;
    private readonly TimeSpan _retryDelay;

    public ConsulRegistrationHostedService(
        IRegistrationService registrationService,
        ILogger<ConsulRegistrationHostedService> logger)
        : this(registrationService, logger, DefaultRetryDelay)
    {
    }

    // Used by tests to shorten the retry interval.
    internal ConsulRegistrationHostedService(
        IRegistrationService registrationService,
        ILogger<ConsulRegistrationHostedService> logger,
        TimeSpan retryDelay)
    {
        _registrationService = registrationService;
        _logger = logger;
        _retryDelay = retryDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register on the background task without blocking host startup: the server listen addresses
        // pushed by the hosting layer only become available while startup proceeds.
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await _registrationService.RegisterAsync(stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // The host is stopping before the registration completed.
                return;
            }
            catch (Exception exception) when (IsPermanentFailure(exception))
            {
                // Invalid configuration cannot be fixed by retrying: fail fast with a single error.
                _logger.LogError(
                    exception,
                    "Failed to register the service instances with Consul because the registration configuration is invalid.");
                return;
            }
            catch (Exception exception)
            {
                // Registration is best effort: a Consul outage must not stop the host.
                _logger.LogError(
                    exception,
                    "Failed to register the service instances with Consul (attempt {Attempt} of {Attempts}).",
                    attempt,
                    MaxRegistrationAttempts);

                if (attempt >= MaxRegistrationAttempts)
                {
                    return;
                }
            }

            try
            {
                await Task.Delay(_retryDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private static bool IsPermanentFailure(Exception exception)
    {
        // Options validation and the internal consistency checks in ServiceInstanceStore fail
        // deterministically, so retrying them only repeats the same error.
        return exception is OptionsValidationException or InvalidOperationException or ArgumentException;
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
