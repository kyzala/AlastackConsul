﻿using Alastack.Consul;
using Alastack.Consul.Registration;
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
    [Obsolete("This method is obsolete. Call AddConsulNaming instead.", false)]
    public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configure)
    {
        return services.AddConsulNaming(configure);
    }

    /// <summary>
    /// Adds services for <see cref="ConsulOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <returns>The service collection.</returns>
    [Obsolete("This method is obsolete. Call AddConsulNaming instead.", false)]
    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration, string key = "Consul")
    {
        return services.AddConsulNaming(configuration, key);
    }

    /// <summary>
    /// Adds services for <see cref="ConsulOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Used to configure the provided <see cref="ConsulOptions"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulNaming(this IServiceCollection services, Action<ConsulOptions> configure)
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
        services.AddConsulCore();
        return services;
    }

    /// <summary>
    /// Adds services for <see cref="ConsulOptions"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulNaming(this IServiceCollection services, IConfiguration configuration, string key = "Consul") 
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
        services.AddConsulCore();        
        return services;        
    }

    private static IServiceCollection AddConsulCore(this IServiceCollection services) 
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ConsulOptions>, ConsulPostConfigureOptions>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ConsulOptions>, ConsulValidateOptions>());
        services.AddSingleton<IRegistrationService, RegistrationService>();
        services.AddHostedService<DynamicRegistrationHostedService>();

        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IServerAddressesHandler, NoneServerAddressesHandler>());

        return services;
    }
}
