using Consul;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

internal class RegistrationService : IRegistrationService
{
    private readonly ConsulRegistrationOptions _registrationOptions;
    private readonly ServiceInstanceStore _instanceStore;
    private readonly Func<IConsulClient> _clientFactory;

    public RegistrationService(
        IOptions<ConsulRegistrationOptions> options,
        ServiceInstanceStore instanceStore,
        Func<IConsulClient> clientFactory)
    {
        _registrationOptions = options.Value;
        _instanceStore = instanceStore;
        _clientFactory = clientFactory;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        using var consulClient = _clientFactory();

        var instances = await _instanceStore.GetInstancesAsync(cancellationToken);
        if (instances.Count == 0)
        {
            // Registering nothing while reporting success makes a misconfiguration invisible.
            throw new InvalidOperationException(
                "No service instances were resolved. Configure Consul:Registration:Instances " +
                "or add the hosting server addresses provider with AddConsulAspNetCore().");
        }

        var meta = BuildServiceMetadata(_registrationOptions.Metadata);

        foreach (var instance in instances)
        {
            var reg = new AgentServiceRegistration()
            {
                ID = instance.Id,
                Name = _registrationOptions.Name,
                Address = instance.Address.Host,
                Port = instance.Address.Port,
                Tags = instance.Tags,
                Meta = meta,
                EnableTagOverride = instance.EnableTagOverride,
                Check = instance.HealthCheck.Enabled
                    ? new AgentServiceCheck
                    {
                        CheckID = instance.HealthCheck.CheckId,
                        Name = instance.HealthCheck.Name,
                        DeregisterCriticalServiceAfter = instance.HealthCheck.DeregisterCriticalServiceAfter,
                        Interval = instance.HealthCheck.Interval,
                        HTTP = instance.HealthCheck.Endpoint.ToString(),
                        Timeout = instance.HealthCheck.Timeout
                    }
                    : null
            };
            await consulClient.Agent.ServiceRegister(reg, cancellationToken);
        }
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken)
    {
        using var consulClient = _clientFactory();

        foreach (var instance in await _instanceStore.GetInstancesAsync(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instance.Id))
            {
                // The id was left for Consul to assign (RegistrationIdNullPolicy = Consul), so it
                // cannot be addressed here. Consul removes the instance through its health check.
                continue;
            }

            await consulClient.Agent.ServiceDeregister(instance.Id, cancellationToken);
        }
    }

    /// <summary>
    /// Copies the configured metadata without the keys that only drive registration behavior,
    /// so internal policies are not published as service metadata in Consul.
    /// </summary>
    private static Dictionary<string, string>? BuildServiceMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata == null || metadata.Count == 0)
        {
            return null;
        }

        var serviceMetadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
        serviceMetadata.Remove(RegistrationPolicies.RegistrationIdNullPolicy);

        return serviceMetadata.Count == 0 ? null : serviceMetadata;
    }

    // Create ConsulClient instance.
    internal static IConsulClient CreateConsulClient(AgentOptions consulAgent)
    {
        return new ConsulClient(config =>
        {
            // Only override the Consul client defaults when the option was actually configured.
            if (!string.IsNullOrWhiteSpace(consulAgent.Datacenter))
            {
                config.Datacenter = consulAgent.Datacenter;
            }
            if (consulAgent.Address != null)
            {
                config.Address = consulAgent.Address;
            }
            if (!string.IsNullOrWhiteSpace(consulAgent.Token))
            {
                config.Token = consulAgent.Token;
            }
            if (consulAgent.WaitTime.HasValue)
            {
                config.WaitTime = consulAgent.WaitTime;
            }
        });
    }
}
