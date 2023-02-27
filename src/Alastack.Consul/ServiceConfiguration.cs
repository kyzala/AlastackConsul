namespace Alastack.Consul;
public sealed class ServiceConfiguration
{
    public string Path { get; set; } = default!;

    public IList<ConfigurationSet>? Sets { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }

    public TimeSpan PollingWaitTime { get; set; } = ConsulOptionsDefaults.Configuration.PollingWaitTime;

    public bool IgnoreException { get; set; }
}
