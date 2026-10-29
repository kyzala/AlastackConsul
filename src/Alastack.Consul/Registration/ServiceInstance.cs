namespace Alastack.Consul.Registration;

/// <summary>
/// Service instance registration settings.
/// </summary>
public class ServiceInstance
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
    /// Configure service Health check.
    /// </summary>
    public HealthCheckOptions? HealthCheck { get; set; } = default!;
}
