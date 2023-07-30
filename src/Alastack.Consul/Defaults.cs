namespace Alastack.Consul;

/// <summary>
/// Service registration defaults.
/// </summary>
public static class ServiceRegistrationDefaults
{    
    /// <summary>
    /// Default CheckInterval for the service registration.
    /// </summary>
    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Default CheckTimeout for the service registration.
    /// </summary>
    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Default version for the service.
    /// </summary>
    public const string Version = "1.0.0";
}

/// <summary>
/// Application configuration defaults.
/// </summary>
public static class AppConfigurationDefaults
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
