namespace Alastack.Consul.Registration;

/// <summary>
/// A service instance with all registration values materialized, ready to be registered to Consul.
/// </summary>
internal sealed class ResolvedServiceInstance
{
    /// <summary>
    /// The service instance id. A <see langword="null"/> value lets Consul assign one.
    /// </summary>
    public string? Id { get; private init; }

    /// <summary>
    /// The service instance listen address.
    /// </summary>
    public required Uri Address { get; init; }

    /// <summary>
    /// The service instance tags.
    /// </summary>
    public string[]? Tags { get; private init; }

    /// <summary>
    /// The enable tag override flag.
    /// </summary>
    public bool EnableTagOverride { get; private init; }

    /// <summary>
    /// The materialized health check.
    /// </summary>
    public required HealthCheckOptions HealthCheck { get; init; }

    /// <summary>
    /// Resolves a configuration <see cref="ServiceInstance"/> to a <see cref="ResolvedServiceInstance"/> with registration defaults applied.
    /// </summary>
    public static ResolvedServiceInstance Resolve(ServiceInstance instance, ConsulRegistrationOptions registration)
    {
        var id = instance.Id;
        if (string.IsNullOrWhiteSpace(id) && IsDefaultIdPolicy(registration.Metadata))
        {
            id = $"{registration.Name}@{instance.Address.Host}:{instance.Address.Port}";
        }

        var source = instance.HealthCheck ?? registration.DefaultHealthCheck;
        var healthCheck = new HealthCheckOptions
        {
            CheckId = source.CheckId,
            Name = source.Name,
            DeregisterCriticalServiceAfter = source.DeregisterCriticalServiceAfter,
            Interval = source.Interval,
            Endpoint = source.Endpoint,
            Timeout = source.Timeout
        };
        if (string.IsNullOrWhiteSpace(healthCheck.Name))
        {
            healthCheck.Name = $"Service '{id}' check";
        }
        if (string.IsNullOrWhiteSpace(healthCheck.CheckId))
        {
            healthCheck.CheckId = $"service:{id}";
        }
        if (!healthCheck.Endpoint.IsAbsoluteUri)
        {
            healthCheck.Endpoint = new Uri(instance.Address, healthCheck.Endpoint);
        }

        return new ResolvedServiceInstance
        {
            Id = id,
            Address = instance.Address,
            Tags = instance.Tags,
            EnableTagOverride = instance.EnableTagOverride,
            HealthCheck = healthCheck
        };
    }

    private static bool IsDefaultIdPolicy(IDictionary<string, string>? metadata)
    {
        if (metadata != null && metadata.TryGetValue(RegistrationPolicies.RegistrationIdNullPolicy, out var value) && RegistrationIdNullPolicy.IsConsul(value))
        {
            return false;
        }
        return true;
    }
}
