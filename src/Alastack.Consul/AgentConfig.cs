namespace Alastack.Consul;
public sealed class AgentConfig
{
    public string Datacenter { get; init; } = default!;

    public Uri Address { get; init; } = default!;

    //public int Port { get; init; }

    public string? Token { get; init; }

    public TimeSpan? WaitTime { get; init; }

    public IDictionary<string, string>? Metadata { get; set; }

}
