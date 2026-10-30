using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConsulAspNetCoreServiceCollectionExtensions
{
    public static IServiceCollection AddConsulAspNetCore(this IServiceCollection services)
    {
        services.TryAddSingleton<KestrelServerAddressesProvider>();
        services.TryAddSingleton<IServerAddressesProvider>(sp => sp.GetRequiredService<KestrelServerAddressesProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ServerAddressesFilter>());

        return services;
    }
}
