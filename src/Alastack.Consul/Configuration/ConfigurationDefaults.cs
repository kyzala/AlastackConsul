namespace Alastack.Consul.Configuration;

/// <summary>
/// Configuration defaults.
/// </summary>
public static class ConfigurationDefaults
{
    /// <summary>
    /// Default Namespace for the application configuration.
    /// </summary>
    public const string Namespace = "public";

    /// <summary>
    /// Default Group for the application configuration.
    /// </summary>
    public const string Group = "default";

    /// <summary>
    /// Default PollingWaitTime for the application configuration.
    /// </summary>
    public static readonly TimeSpan PollingWaitTime = TimeSpan.FromSeconds(5);
}
