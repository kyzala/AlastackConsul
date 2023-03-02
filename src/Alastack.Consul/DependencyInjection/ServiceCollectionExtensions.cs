using Alastack.Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ConsulOptions"/>.
/// </summary>
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
