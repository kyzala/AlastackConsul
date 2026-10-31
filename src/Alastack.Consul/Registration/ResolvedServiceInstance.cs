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
        ArgumentNullException.ThrowIfNull(instance);
        if (instance.Address == null)
        {
            throw new InvalidOperationException("The service instance address is required.");
        }

        var id = string.IsNullOrWhiteSpace(instance.Id) ? null : instance.Id;
        if (id == null && IsDefaultIdPolicy(registration.Metadata))
        {
            id = $"{registration.Name}@{instance.Address.Host}:{instance.Address.Port}";
        }

        var source = instance.HealthCheck ?? registration.DefaultHealthCheck;
        var healthCheck = new HealthCheckOptions
        {
            Enabled = source.Enabled,
            CheckId = source.CheckId,
            Name = source.Name,
            DeregisterCriticalServiceAfter = source.DeregisterCriticalServiceAfter,
            Interval = source.Interval,
            Endpoint = source.Endpoint,
            Timeout = source.Timeout,
            TLSSkipVerify = source.TLSSkipVerify
        };

        if (!healthCheck.Enabled)
        {
            return new ResolvedServiceInstance
            {
                Id = id,
                Address = instance.Address,
                Tags = instance.Tags,
                EnableTagOverride = instance.EnableTagOverride,
                HealthCheck = healthCheck
            };
        }

        if (string.IsNullOrWhiteSpace(healthCheck.Name))
        {
            // When the id is left to Consul there is no id to derive a unique check from, so use the
            // address: check ids must be unique on the Consul agent, even across multiple instances.
            healthCheck.Name = id == null
                ? $"service:{instance.Address.DnsSafeHost}:{instance.Address.Port}"
                : $"Service '{id}' check";
        }
        if (string.IsNullOrWhiteSpace(healthCheck.CheckId))
        {
            healthCheck.CheckId = id == null
                ? $"service:{instance.Address.DnsSafeHost}:{instance.Address.Port}"
                : $"service:{id}";
        }
        if (healthCheck.Endpoint == null)
        {
            throw new InvalidOperationException($"The health check endpoint for the service instance '{id}' is required.");
        }
        if (!healthCheck.Endpoint.IsAbsoluteUri)
        {
            try
            {
                healthCheck.Endpoint = new Uri(instance.Address, healthCheck.Endpoint);
            }
            catch (UriFormatException exception)
            {
                throw new InvalidOperationException(
                    $"The health check endpoint '{healthCheck.Endpoint}' of the service instance '{id}' is not a valid URI.",
                    exception);
            }
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
