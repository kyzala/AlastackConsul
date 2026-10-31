namespace Alastack.Consul.Registration;

/// <summary>
/// Provides service registration and deregistration against the Consul agent.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers the resolved service instances to Consul.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RegisterAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Deregisters the resolved service instances from Consul.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeregisterAsync(CancellationToken cancellationToken);
}
