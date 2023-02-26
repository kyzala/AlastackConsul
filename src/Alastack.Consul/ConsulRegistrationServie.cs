using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alastack.Consul
{
    public class ConsulRegistrationServie : IHostedService
    {
        private ConsulOptions _consulOptions;
        //private IConsulClient _consulClient;

        public ConsulRegistrationServie(IOptions<ConsulOptions> options) 
        {
            _consulOptions = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var appConfig = _consulOptions.Application;
            var registration = appConfig.Registration ?? throw new ArgumentNullException(nameof(ServiceRegistration));

            var consulClient = CreateConsulClient(_consulOptions);

            await consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = registration.Id ?? appConfig.BuildRegistrationId(),
                Name = registration.Name ?? appConfig.BuildRegistrationName(),
                Address = registration.Address.Host,
                Port = registration.Address.Port,
                Tags = registration.Tags,
                Meta = registration.Metadata,
                EnableTagOverride = registration.EnableTagOverride,
                Check = new AgentServiceCheck
                {
                    //ID = "service:WebApi:1.0.0#127.0.0.1:7001",
                    Name = registration.HealthCheck.Name ?? appConfig.BuildHealthCheckName(),
                    DeregisterCriticalServiceAfter = registration.HealthCheck.DeregisterCriticalServiceAfter,
                    Interval = registration.HealthCheck.Interval,
                    HTTP = appConfig.BuildHealthCheckAddress(),
                    Timeout = registration.HealthCheck.Timeout
                }
                //Checks = consulOptions.Application.Registration.Checks
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var appConfig = _consulOptions.Application;
            var registration = appConfig.Registration ?? throw new ArgumentNullException(nameof(ServiceRegistration));
            var consulClient = CreateConsulClient(_consulOptions); 
            await consulClient.Agent.ServiceDeregister(registration.Id ?? appConfig.BuildRegistrationId());
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
