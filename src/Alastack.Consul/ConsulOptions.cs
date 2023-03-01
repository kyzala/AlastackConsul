namespace Alastack.Consul;

public sealed class ConsulOptions
{
    public ConsulAgent Agent { get; set; } = default!;

    public AppConfiguration? Configuration { get; set; }

    public ServiceRegistration? Registration { get; set; }
}