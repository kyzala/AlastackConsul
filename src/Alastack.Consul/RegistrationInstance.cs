namespace Alastack.Consul;

/// <summary>
/// Registration instance
/// </summary>
public class RegistrationInstance
{
    /// <summary>
    /// Configure service id.
    /// </summary>
    public string? Id { get; set; }

    
    /// <summary>
    /// Configure service addresses.
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
    //public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Configure service Health check.
    /// </summary>
    public ServiceHealthCheck? HealthCheck { get; set; } = default!;
}
