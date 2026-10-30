using Consul;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

internal class RegistrationService : IRegistrationService
{
    private readonly ConsulRegistrationOptions _registrationOptions;

    public RegistrationService(IOptions<ConsulRegistrationOptions> options)
    {
        _registrationOptions = options.Value;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var consulClient = CreateConsulClient(_registrationOptions.Agent);

        foreach (var instance in _registrationOptions.Instances)
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
                Check = new AgentServiceCheck
                {
                    CheckID = instance.HealthCheck!.CheckId,
                    Name = instance.HealthCheck.Name,
                    DeregisterCriticalServiceAfter = instance.HealthCheck.DeregisterCriticalServiceAfter,
                    Interval = instance.HealthCheck.Interval,
                    HTTP = instance.HealthCheck.Endpoint.ToString(),
                    Timeout = instance.HealthCheck.Timeout
                }
            };
            await consulClient.Agent.ServiceRegister(reg, cancellationToken);
        }

        consulClient.Dispose();
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken)
    {
        var consulClient = CreateConsulClient(_registrationOptions.Agent);
        foreach (var instance in _registrationOptions.Instances)
        {
            await consulClient.Agent.ServiceDeregister(instance.Id, cancellationToken);
        }

        consulClient.Dispose();
    }

    // Create ConsulClient instance.
    private static IConsulClient CreateConsulClient(AgentOptions consulAgent)
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
