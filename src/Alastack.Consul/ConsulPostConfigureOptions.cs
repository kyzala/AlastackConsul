using Microsoft.Extensions.Options;

namespace Alastack.Consul;

/// <summary>
/// Used to setup defaults for the <see cref="ConsulOptions"/>.
/// </summary>
public class ConsulPostConfigureOptions : IPostConfigureOptions<ConsulOptions>
{
    /// <summary>
    /// Invoked to post configure a <see cref="ConsulOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConsulOptions"/> instance being configured.</param>
    /// <param name="options">The <see cref="ConsulOptions"/> instance to configure.</param>
    public void PostConfigure(string? name, ConsulOptions options)
    {       
        if (options.Registration != null) 
        {

            foreach (var instance in options.Registration.Instances) 
            {
                if (String.IsNullOrWhiteSpace(instance.Id))
                {
                    instance.Id = instance.BuildRegistrationInstanceId(options.Registration.Name, options.Registration.Metadata);
                }

                if (instance.HealthCheck == null) 
                {
                    instance.HealthCheck = options.Registration.HealthCheckDefault;
                }

                if (String.IsNullOrWhiteSpace(instance.HealthCheck.Name))
                {
                    instance.HealthCheck.Name = $"Service '{options.Registration.Name}' check";
                }

                if (String.IsNullOrWhiteSpace(instance.HealthCheck.CheckId))
                {
                    instance.HealthCheck.CheckId = $"service:{instance.Id}";
                }

                if (!instance.HealthCheck.Health.IsAbsoluteUri)
                {
                    instance.HealthCheck.Health = new Uri(instance.Address, instance.HealthCheck.Health);
                }
            }
        }
    }
}
