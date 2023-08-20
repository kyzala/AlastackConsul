namespace Alastack.Consul;

/// <summary>
/// Consul options for Service Discovery and Configuration.
/// </summary>
public sealed class ConsulOptions    
{
    /// <summary>
    /// Gets or sets flag indicating if the consulOptions is enabled. Default is <c>true</c>.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Agent configuration settings.
    /// </summary>
    public AgentConfiguration Agent { get; set; } = default!;

    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public AppConfiguration? Configuration { get; set; }

    /// <summary>
    /// Service registration settings.
    /// </summary>
    public ServiceRegistration? Registration { get; set; }
}