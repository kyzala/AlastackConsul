namespace Alastack.Consul;
public sealed class ServiceHealthCheck
{
    public string? CheckId { get; set; }

    public string? Name { get; set; }

    public TimeSpan? DeregisterCriticalServiceAfter { get; set; } = ConsulRegistrationDefaults.DeregisterCriticalServiceAfter;

    public TimeSpan? Interval { get; set; } = ConsulRegistrationDefaults.CheckInterval;

    public string Health { get; set; } = default!;

    public TimeSpan? Timeout { get; set; } = ConsulRegistrationDefaults.CheckTimeout;



}
