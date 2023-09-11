namespace Alastack.Consul;

/// <summary>
/// Service registration for service discovery.
/// </summary>
public sealed class ServiceRegistration
{
    /// <summary>
    /// Configure service name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Configure service Version.
    /// </summary>
    public string Version { get; set; } = ServiceRegistrationDefaults.Version;

    /// <summary>
    /// Registration instances
    /// </summary>
    public ICollection<RegistrationInstance> Instances { get; set; } = default!;

    /// <summary>
    /// ServiceHealthCheck default to <see cref="RegistrationInstance.HealthCheck"/>.
    /// </summary>
    public ServiceHealthCheck HealthCheckDefault { get; set; } = new ServiceHealthCheck();

    /// <summary>
    /// Metadata for the service.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }

}
