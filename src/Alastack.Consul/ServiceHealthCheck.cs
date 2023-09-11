using System;

namespace Alastack.Consul;

/// <summary>
/// ServiceHealthCheck is used to create an associated check for a service.
/// </summary>
public sealed class ServiceHealthCheck
{
    /// <summary>
    /// Service health check id.
    /// </summary>
    public string? CheckId { get; set; }

    /// <summary>
    /// Service health check name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// If a health check is in the critical state for more than this configured value,
    /// then its associated service (and all of its associated checks) will automatically be deregistered.
    /// </summary>
    public TimeSpan? DeregisterCriticalServiceAfter { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Service health check interval.
    /// </summary>
    public TimeSpan? Interval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Service health check endpoint.
    /// </summary>
    public Uri Health { get; set; } = new Uri("/health");

    /// <summary>
    ///  Service health timeout value.
    /// </summary>
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(10);



}
