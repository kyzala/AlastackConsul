namespace Alastack.Consul;

/// <summary>
/// Application configuration definition.
/// </summary>
public sealed class AppConfiguration
{
    /// <summary>
    /// Base path for the application configuration stored in consul KV.
    /// </summary>
    public string PathBase { get; set; } = default!;

    /// <summary>
    /// The application configuration namespace. Defaults to <c>ns.default</c>.
    /// </summary>
    public string Namespace { get; set; } = AppConfigurationDefaults.Namespace;

    /// <summary>
    /// The application <see cref="ConfigurationSet"/> list.
    /// </summary>
    public IList<ConfigurationSet>? Sets { get; set; }

    /// <summary>
    /// Metadata for the application configuration. Support custom extension.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }
}
