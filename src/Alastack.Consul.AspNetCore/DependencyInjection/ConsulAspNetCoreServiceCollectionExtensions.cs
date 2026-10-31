using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConsulAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds the server addresses integration that lets the registration hosted service discover
    /// the addresses the application is listening on.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulAspNetCore(this IServiceCollection services)
        => services.AddConsulAspNetCore(null);

    /// <summary>
    /// Adds the server addresses integration that lets the registration hosted service discover
    /// the addresses the application is listening on.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serverAddressesTimeout">
    /// The maximum time to wait for the hosting layer to publish the server addresses.
    /// Defaults to <see cref="RegistrationDefaults.ServerAddressesTimeout"/>.
    /// </param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConsulAspNetCore(this IServiceCollection services, TimeSpan? serverAddressesTimeout)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (serverAddressesTimeout.HasValue && serverAddressesTimeout.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(serverAddressesTimeout),
                serverAddressesTimeout.Value,
                "The server addresses timeout must be greater than zero.");
        }

        services.TryAddSingleton<DefaultServerAddressesProvider>(
            _ => new DefaultServerAddressesProvider(serverAddressesTimeout ?? RegistrationDefaults.ServerAddressesTimeout));
        services.TryAddSingleton<IServerAddressesProvider>(sp => sp.GetRequiredService<DefaultServerAddressesProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ServerAddressesInitializer>());

        return services;
    }
}
