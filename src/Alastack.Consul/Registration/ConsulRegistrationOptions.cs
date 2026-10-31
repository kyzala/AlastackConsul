using System.Collections.ObjectModel;

namespace Alastack.Consul.Registration;

/// <summary>
/// Service registration options for service discovery.
/// </summary>
public sealed class ConsulRegistrationOptions
{
    /// <summary>
    /// Consul agent options.
    /// </summary>
    public AgentOptions Agent { get; set; } = new();

    /// <summary>
    /// Configure service name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Configure service Version.
    /// </summary>
    public string Version { get; set; } = RegistrationDefaults.Version;

    /// <summary>
    /// Registration instances
    /// </summary>
    public ICollection<ServiceInstance> Instances { get; set; } = new Collection<ServiceInstance>();

    /// <summary>
    /// Gets or sets a value indicating whether the server listen addresses pushed by the hosting
    /// layer are registered automatically. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeServerAddresses { get; set; } = true;

    /// <summary>
    /// Health check default for <see cref="ServiceInstance.HealthCheck"/>.
    /// </summary>
    public HealthCheckOptions DefaultHealthCheck { get; set; } = new HealthCheckOptions();

    /// <summary>
    /// Metadata for the service.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }
}
