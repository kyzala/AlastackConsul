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

        foreach (var instance in await _instanceStore.GetInstancesAsync(cancellationToken))
        {
            var reg = new AgentServiceRegistration()
            {
                ID = instance.Id,
                Name = _registrationOptions.Name,
                Address = instance.Address.Host,
                Port = instance.Address.Port,
                Tags = instance.Tags,
                Meta = _registrationOptions.Metadata,
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
            await consulClient.Agent.ServiceDeregister(instance.Id, cancellationToken);
        }
    }

    // Create ConsulClient instance.
    internal static IConsulClient CreateConsulClient(AgentOptions consulAgent)
    {
        return new ConsulClient(config =>
        {
            config.Datacenter = consulAgent.Datacenter;
            config.Address = consulAgent.Address;
            config.Token = consulAgent.Token;
            config.WaitTime = consulAgent.WaitTime;
        });
    }
}
