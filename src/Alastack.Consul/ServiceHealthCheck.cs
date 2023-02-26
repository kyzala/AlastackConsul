namespace Alastack.Consul;
public sealed class ServiceHealthCheck
{
    //public string? Id { get; set; }
    public string? Name { get; set; }

    public TimeSpan? DeregisterCriticalServiceAfter { get; set; }

    public TimeSpan? Interval { get; set; }

    public string Health { get; set; } = default!;

    public TimeSpan? Timeout { get; set; }



}
