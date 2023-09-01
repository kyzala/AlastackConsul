using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul;

/// <summary>
/// Hosted servie for the service registration.
/// </summary>
public class ServiceRegistrationService : IHostedService
{
    private readonly ConsulOptions _consulOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceRegistrationService"/>.
    /// </summary>
    /// <param name="options"><see cref="ConsulOptions"/></param>    
    public ServiceRegistrationService(IOptions<ConsulOptions> options) 
    {
        //if (options.Value.Registration == null) 
        //{
        //    throw new ArgumentNullException(nameof(IOptions<ConsulOptions>.Value.Registration));
        //}
        _consulOptions = options.Value;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_consulOptions.Registration != null)
        {
            var registration = _consulOptions.Registration!;

            var consulClient = CreateConsulClient(_consulOptions.Agent);

            await consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = registration.Id,
                Name = registration.Name,
                Address = registration.Address.Host,
                Port = registration.Address.Port,
                Tags = registration.Tags,
                Meta = registration.Metadata,
                EnableTagOverride = registration.EnableTagOverride,
                Check = new AgentServiceCheck
                {
                    CheckID = registration.HealthCheck.CheckId,
                    Name = registration.HealthCheck.Name,
                    DeregisterCriticalServiceAfter = registration.HealthCheck.DeregisterCriticalServiceAfter,
                    Interval = registration.HealthCheck.Interval,
                    HTTP = registration.HealthCheck.Health.ToString(),
                    Timeout = registration.HealthCheck.Timeout
                }
                //Checks = registration.Checks
            }, cancellationToken);
            consulClient.Dispose();
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consulOptions.Registration != null)
        {
            var consulClient = CreateConsulClient(_consulOptions.Agent);
            await consulClient.Agent.ServiceDeregister(_consulOptions.Registration!.Id, cancellationToken);
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
