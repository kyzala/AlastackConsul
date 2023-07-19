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
            if (String.IsNullOrWhiteSpace(options.Registration.Id)) 
            {
                options.Registration.Id = options.Registration.BuildRegistrationId();
            }
            
            if (String.IsNullOrWhiteSpace(options.Registration.HealthCheck.Name))
            {
                options.Registration.HealthCheck.Name = $"Service '{options.Registration.Name}' check";
            }

            if (String.IsNullOrWhiteSpace(options.Registration.HealthCheck.CheckId))
            {
                options.Registration.HealthCheck.CheckId = $"service:{options.Registration.Id}";
            }

            if (!options.Registration.HealthCheck.Health.IsAbsoluteUri) 
            {
                options.Registration.HealthCheck.Health = new Uri(options.Registration.Address, options.Registration.HealthCheck.Health);
            }
            
        }
    }
}
