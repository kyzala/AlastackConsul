using Consul;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

public class RegistrationService : IRegistrationService
{
    private readonly ConsulOptions _consulOptions;

    public RegistrationService(IOptions<ConsulOptions> options)
    {
        _consulOptions = options.Value;
    }

    public async Task ServiceRegister(CancellationToken cancellationToken)
    {
        if (_consulOptions.Registration != null)
        {
            var registration = _consulOptions.Registration;

            var consulClient = CreateConsulClient(_consulOptions.Agent);

            foreach (var instance in registration.Instances)
            {
                var reg = new AgentServiceRegistration()
                {
                    ID = instance.Id,
                    Name = registration.Name,
                    Address = instance.Address.Host,
                    Port = instance.Address.Port,
                    Tags = instance.Tags,
                    Meta = registration.Metadata,
                    EnableTagOverride = instance.EnableTagOverride,
                    Check = new AgentServiceCheck
                    {
                        CheckID = instance.HealthCheck!.CheckId,
                        Name = instance.HealthCheck.Name,
                        DeregisterCriticalServiceAfter = instance.HealthCheck.DeregisterCriticalServiceAfter,
                        Interval = instance.HealthCheck.Interval,
                        HTTP = instance.HealthCheck.Health.ToString(),
                        Timeout = instance.HealthCheck.Timeout
                    }
                    //Checks = registration.Checks
                };
                await consulClient.Agent.ServiceRegister(reg, cancellationToken);
            }

            consulClient.Dispose();
        }
    }

    public async Task ServiceDeregister(CancellationToken cancellationToken)
    {
        if (_consulOptions.Registration != null)
        {
            var consulClient = CreateConsulClient(_consulOptions.Agent);
            foreach (var instance in _consulOptions.Registration.Instances)
            {
                await consulClient.Agent.ServiceDeregister(instance.Id, cancellationToken);
            }

            consulClient.Dispose();
        }
    }

    // Create ConsulClient instance.
    private static IConsulClient CreateConsulClient(AgentConfiguration consulAgent)
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
