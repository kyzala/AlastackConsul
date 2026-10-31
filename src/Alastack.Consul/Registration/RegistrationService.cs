using Alastack.Extensions.Configuration.Consul;
using Consul;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

internal class RegistrationService : IRegistrationService
{
    private readonly ConsulRegistrationOptions _registrationOptions;
    private readonly ServiceInstanceStore _instanceStore;
    private readonly IConsulClientFactory _clientFactory;

    public RegistrationService(
        IOptions<ConsulRegistrationOptions> options,
        ServiceInstanceStore instanceStore,
        IConsulClientFactory clientFactory)
    {
        _registrationOptions = options.Value;
        _instanceStore = instanceStore;
        _clientFactory = clientFactory;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        // The client is shared by all callers with the same agent settings, so it must not be disposed here.
        var consulClient = _clientFactory.GetClient(_registrationOptions.Agent.ToConsulClientSettings());

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
                Address = instance.Address.DnsSafeHost,
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
                        Timeout = instance.HealthCheck.Timeout,
                        TLSSkipVerify = instance.HealthCheck.TLSSkipVerify
                    }
                    : null
            };
            await consulClient.Agent.ServiceRegister(reg, cancellationToken);
        }
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken)
    {
        // The client is shared by all callers with the same agent settings, so it must not be disposed here.
        var consulClient = _clientFactory.GetClient(_registrationOptions.Agent.ToConsulClientSettings());

        var exceptions = new List<Exception>();
        foreach (var instance in await _instanceStore.GetInstancesAsync(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instance.Id))
            {
                // The id was left for Consul to assign (RegistrationIdNullPolicy = Consul), so it
                // cannot be addressed here. Consul removes the instance through its health check.
                continue;
            }

            try
            {
                await consulClient.Agent.ServiceDeregister(instance.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                // A failing instance must not leave the remaining instances registered as ghosts.
                exceptions.Add(exception);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Failed to deregister one or more service instances from Consul.", exceptions);
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
}
