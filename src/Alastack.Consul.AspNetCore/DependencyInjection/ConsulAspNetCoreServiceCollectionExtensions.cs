using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConsulAspNetCoreServiceCollectionExtensions
{
    public static IServiceCollection AddConsulAspNetCore(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<DefaultServerAddressesProvider>();
        services.TryAddSingleton<IServerAddressesProvider>(sp => sp.GetRequiredService<DefaultServerAddressesProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ServerAddressesInitializer>());

        return services;
    }
}
