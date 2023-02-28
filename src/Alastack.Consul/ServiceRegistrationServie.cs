using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul
{
    public class ServiceRegistrationServie : IHostedService
    {
        private readonly ConsulOptions _consulOptions;
        private readonly IConsulClient _consulClient;

        public ServiceRegistrationServie(IOptions<ConsulOptions> options) 
        {
            _consulOptions = options.Value;
            _consulClient = CreateConsulClient(_consulOptions);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var registration = _consulOptions.Registration ?? throw new ArgumentNullException(nameof(ServiceRegistration));

            //var consulClient = CreateConsulClient(_consulOptions);

            await _consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = registration.Id ?? registration.BuildRegistrationId(),
                Name = registration.Name ?? registration.BuildRegistrationName(),
                Address = registration.Address.Host,
                Port = registration.Address.Port,
                Tags = registration.Tags,
                Meta = registration.Metadata,
                EnableTagOverride = registration.EnableTagOverride,
                Check = new AgentServiceCheck
                {
                    //ID = "service:WebApi:1.0.0#127.0.0.1:7001",
                    Name = registration.HealthCheck.Name ?? registration.BuildHealthCheckName(),
                    DeregisterCriticalServiceAfter = registration.HealthCheck.DeregisterCriticalServiceAfter,
                    Interval = registration.HealthCheck.Interval,
                    HTTP = registration.BuildHealthCheckAddress(),
                    Timeout = registration.HealthCheck.Timeout
                }
                //Checks = consulOptions.Application.Registration.Checks
            });
            //consulClient.Dispose();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {            
            var registration = _consulOptions.Registration ?? throw new ArgumentNullException(nameof(ServiceRegistration));
            //var consulClient = CreateConsulClient(_consulOptions); 
            await _consulClient.Agent.ServiceDeregister(registration.Id ?? registration.BuildRegistrationId());
            //consulClient.Dispose();
        }

        // Create Consul agent
        private static IConsulClient CreateConsulClient(ConsulOptions consulOptions) 
        {            
            return new ConsulClient(config =>
            {
                config.Datacenter = consulOptions.Agent.Datacenter;
                config.Address = consulOptions.Agent.Address;
                config.Token = consulOptions.Agent.Token;
                config.WaitTime = consulOptions.Agent.WaitTime;
            });
        }
    }
}
