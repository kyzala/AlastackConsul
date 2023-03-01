namespace Alastack.Consul;
public sealed class ConfigurationSet
{
    public string Id { get; set; } = default!;

    public string Group { get; set; } = ConsulConfigurationDefaults.Group;

    public string? Description { get; set; }

    public bool Optional { get; set; } = ConsulConfigurationDefaults.Optional;

    public bool ReloadOnChange { get; set; } = ConsulConfigurationDefaults.ReloadOnChange;

    public TimeSpan PollingWaitTime { get; set; } = ConsulConfigurationDefaults.PollingWaitTime;

    public bool IgnoreException { get; set; } = ConsulConfigurationDefaults.IgnoreException;
}
