namespace Alastack.Consul;

/// <summary>
/// Service registration for service discovery.
/// </summary>
public sealed class ServiceRegistration
{
    /// <summary>
    /// Configure service id.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Configure service name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Configure service Version.
    /// </summary>
    public string Version { get; set; } = ServiceRegistrationDefaults.Version;

    /// <summary>
    /// Configure service address.
    /// </summary>
    public Uri Address { get; set; } = default!;

    /// <summary>
    /// Configure service tags.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Configure Enable tag override flag.
    /// </summary>
    public bool EnableTagOverride { get; set; }

    /// <summary>
    /// Metadata for the service.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Configure service Health check.
    /// </summary>
    public ServiceHealthCheck HealthCheck { get; set; } = default!;

}
