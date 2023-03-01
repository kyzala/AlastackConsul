using Microsoft.Extensions.Options;

namespace Alastack.Consul;

public class ConsulPostConfigureOptions : IPostConfigureOptions<ConsulOptions>
{
    public void PostConfigure(string? name, ConsulOptions options)
    {
        if (options.Registration == null && options.Configuration == null) 
        {
            throw new ArgumentNullException(nameof(options), "Registration or Configuration null.");
        }
        if (options.Registration != null) 
        {
            options.Registration.Id ??= options.Registration.BuildRegistrationId();
            options.Registration.Name ??= options.Registration.BuildRegistrationName();

            options.Registration.HealthCheck.CheckId ??= options.Registration.BuildHealthCheckId();
            options.Registration.HealthCheck.Name ??= options.Registration.BuildHealthCheckName();
        }
    }
}
