namespace Alastack.Consul;

public sealed class ConsulOptions
{
    public AgentConfig Agent { get; set; } = default!;

    public AppConfig Application { get; set; } = default!;
}