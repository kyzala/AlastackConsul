namespace Alastack.Consul;
public sealed class ConsulAgent
{
    public string Datacenter { get; init; } = default!;

    public Uri Address { get; init; } = default!;

    public string? Token { get; init; }

    public TimeSpan? WaitTime { get; init; }

    public IDictionary<string, string>? Metadata { get; set; }

}
