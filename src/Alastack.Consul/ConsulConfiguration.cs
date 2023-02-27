﻿namespace Alastack.Consul;
public sealed class ConsulConfiguration
{
    public string PathBase { get; set; } = default!;

    public string Namespace { get; set; } = ConsulConfigurationDefaults.Namespace;

    public IList<ConfigurationSet>? Sets { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }

    public TimeSpan PollingWaitTime { get; set; } = ConsulConfigurationDefaults.PollingWaitTime;

    public bool IgnoreException { get; set; } = ConsulConfigurationDefaults.IgnoreException;
}