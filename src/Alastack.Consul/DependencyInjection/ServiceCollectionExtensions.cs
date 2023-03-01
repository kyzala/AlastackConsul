using Alastack.Consul;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{    
    public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        services.Configure(configureOptions);
        return services;
    }
    
    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration, string key = "Consul") 
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        services.Configure<ConsulOptions>(configuration.GetSection(key));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ConsulOptions>, ConsulPostConfigureOptions>());
        services.AddHostedService<ServiceRegistrationServie>();
        return services;
    }
}
