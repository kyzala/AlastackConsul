namespace Alastack.Consul.Registration;

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

    /// <summary>
    /// The default timeout in milliseconds for waiting server listen addresses to be configured.
    /// </summary>
    public static readonly int ServerAddressesTimeout = 5000;
}
