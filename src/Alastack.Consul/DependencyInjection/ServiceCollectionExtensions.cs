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
    /// <summary>
    /// Adds services for <see cref="ConsulOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Used to configure the provided <see cref="ConsulOptions"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        return services;
    }

    /// <summary>
    /// Adds services for <see cref="ConsulOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <returns>The service collection.</returns>
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
