namespace Alastack.Consul;

/// <summary>
/// Service registration defaults.
/// </summary>
public static class ServiceRegistrationDefaults
{
    /// <summary>
    /// Default DeregisterCriticalServiceAfter for the service registration.
    /// </summary>
    public static readonly TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Default CheckInterval for the service registration.
    /// </summary>
    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Default CheckTimeout for the service registration.
    /// </summary>
    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Application configuration defaults.
/// </summary>
public static class AppConfigurationDefaults
{
    /// <summary>
    /// Default Namespace for the application configuration.
    /// </summary>
    public const string Namespace = "ns.default";

    /// <summary>
    /// Default Group for the application configuration.
    /// </summary>
    public const string Group = "gp.default";

    //public const bool Optional = true;

    //public const bool ReloadOnChange = true;

    //public const bool IgnoreException = false;

    /// <summary>
    /// Default PollingWaitTime for the application configuration.
    /// </summary>
    public static readonly TimeSpan PollingWaitTime = TimeSpan.FromSeconds(5);

}
