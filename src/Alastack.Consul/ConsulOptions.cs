namespace Alastack.Consul;

public sealed class ConsulOptions
{
    public AgentConfig Agent { get; set; } = default!;

    public ConsulConfiguration? Configuration { get; set; }

    public ServiceRegistration? Registration { get; set; }
}