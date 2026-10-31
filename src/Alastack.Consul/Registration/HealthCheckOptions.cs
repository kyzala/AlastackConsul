using System;

namespace Alastack.Consul.Registration;

/// <summary>
/// Health check options used to create an associated check for a service.
/// </summary>
public sealed class HealthCheckOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether a health check is registered for the service instance.
    /// </summary>
    public bool Enabled { get; set; } = true;

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
    public TimeSpan? DeregisterCriticalServiceAfter { get; set; } = RegistrationDefaults.DeregisterCriticalServiceAfter;

    /// <summary>
    /// Service health check interval.
    /// </summary>
    public TimeSpan? Interval { get; set; } = RegistrationDefaults.Interval;

    /// <summary>
    /// Service health check endpoint.
    /// </summary>
    public Uri Endpoint { get; set; } = new Uri("/health", UriKind.Relative);

    /// <summary>
    ///  Service health timeout value.
    /// </summary>
    public TimeSpan? Timeout { get; set; } = RegistrationDefaults.Timeout;

    /// <summary>
    /// Gets or sets a value indicating whether the certificate of an HTTPS health check endpoint
    /// should not be verified. Useful for self-signed certificates in development environments.
    /// </summary>
    public bool TLSSkipVerify { get; set; }
}
