namespace Alastack.Consul;

/// <summary>
/// Service registration defaults.
/// </summary>
public static class RegistrationDefaults
{
    /// <summary>
    /// Default health check interval for the service registration.
    /// </summary>
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Default health check timeout for the service registration.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Default DeregisterCriticalServiceAfter for the service health check.
    /// </summary>
    public static readonly TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Default version for the service.
    /// </summary>
    public const string Version = "1.0.0";
}

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
