﻿namespace Alastack.Consul;
public sealed class ServiceRegistration
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Version { get; set; }
    
    public Uri Address { get; set; } = default!;

    public string[]? Tags { get; set; }

    public bool EnableTagOverride { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }

    public ServiceHealthCheck HealthCheck { get; set; } = default!;

}
