namespace Alastack.Consul.Configuration;

/// <summary>
/// Options for the application configuration stored in Consul KV.
/// </summary>
public sealed class ConsulConfigurationOptions
{
    /// <summary>
    /// Consul agent options.
    /// </summary>
    public AgentOptions Agent { get; set; } = new();

    /// <summary>
    /// Base path for the application configuration stored in consul KV.
    /// </summary>
    public string PathBase { get; set; } = default!;

    /// <summary>
    /// The application configuration namespace. Defaults to <c>public</c>.
    /// </summary>
    public string Namespace { get; set; } = ConfigurationDefaults.Namespace;

    /// <summary>
    /// The application <see cref="ConfigurationSet"/> list.
    /// </summary>
    public IList<ConfigurationSet>? Sets { get; set; }

    /// <summary>
    /// Metadata for the application configuration. Support custom extension.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }
}
