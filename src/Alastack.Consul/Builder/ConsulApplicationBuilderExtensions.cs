//using Consul;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Options;

//namespace Alastack.Consul;
//public static class ConsulApplicationBuilderExtensions
//{
//    // IHostApplicationLifetime Event
//    public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
//    {
//        var monitorOptions = app.ApplicationServices.GetService<IOptionsMonitor<ConsulOptions>>();
//        var consulOptions = monitorOptions!.CurrentValue;
//        var registration = consulOptions.Application.Registration ?? throw new ArgumentNullException(nameof(ServiceRegistration));

//        // Create Consul agent
//        var consulClient = new ConsulClient(config =>
//        {
//            config.Datacenter = consulOptions.Agent.Datacenter;
//            config.Address = consulOptions.Agent.Address;            
//            config.Token = consulOptions.Agent.Token;
//            config.WaitTime = consulOptions.Agent.WaitTime;
//        });
        
//        // Service Register        
//        consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
//        {
//            ID = registration.Id ?? consulOptions.Application.BuildRegistrationId(),
//            Name = registration.Name ?? consulOptions.Application.BuildRegistrationName(),
//            Address = registration.Address.Host,
//            Port = registration.Address.Port,
//            Tags = registration.Tags,
//            Meta = registration.Metadata,
//            EnableTagOverride = registration.EnableTagOverride,
//            Check = new AgentServiceCheck
//            {
//                //ID = "service:WebApi:1.0.0#127.0.0.1:7001",
//                Name = registration.HealthCheck.Name ?? consulOptions.Application.BuildHealthCheckName(),
//                DeregisterCriticalServiceAfter = registration.HealthCheck.DeregisterCriticalServiceAfter,
//                Interval = registration.HealthCheck.Interval,
//                HTTP = consulOptions.Application.BuildHealthCheckAddress(),
//                Timeout = registration.HealthCheck.Timeout
//            }
//            //Checks = registryCenterOptions.Server.Registration.Checks
//        });

//        // Service Deregister
//        var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
//        lifetime!.ApplicationStopped.Register(async () =>
//        {
//            await consulClient.Agent.ServiceDeregister(registration.Id ?? consulOptions.Application.BuildRegistrationId());
//        });

//        return app;
//    }
//}