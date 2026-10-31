using Alastack.Consul;
using Alastack.Consul.Registration;
using Alastack.Extensions.Configuration.Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ConsulRegistrationOptions"/>.
/// </summary>
public static class ConsulServiceCollectionExtensions
{
    /// <summary>
    /// Adds services for <see cref="ConsulRegistrationOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Used to configure the provided <see cref="ConsulRegistrationOptions"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulRegistration(this IServiceCollection services, Action<ConsulRegistrationOptions> configure)
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
        services.AddConsulRegistrationCore();
        return services;
    }

    /// <summary>
    /// Adds services for <see cref="ConsulRegistrationOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// The <c>{key}:Registration</c> section binds to the registration settings and the shared
    /// <c>{key}:Agent</c> section binds to <see cref="ConsulRegistrationOptions.Agent"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulRegistration(this IServiceCollection services, IConfiguration configuration, string key = "Consul")
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

        var root = configuration.GetSection(key);
        services.AddOptions<ConsulRegistrationOptions>().Configure(options =>
        {
            root.GetSection("Registration").Bind(options);
            root.GetSection("Agent").Bind(options.Agent);
        });
        services.AddConsulRegistrationCore();
        return services;
    }

    private static IServiceCollection AddConsulRegistrationCore(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ConsulRegistrationOptions>, ConsulRegistrationValidateOptions>());
        services.TryAddSingleton<ServiceInstanceStore>();

        // Share one client per agent settings with the configuration sources; a custom factory
        // registered before this call takes precedence.
        services.TryAddSingleton<IConsulClientFactory>(ConsulClientFactory.Default);
        services.TryAddSingleton<IRegistrationService, RegistrationService>();
        services.AddHostedService<ConsulRegistrationHostedService>();

        return services;
    }
}
