using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConsulAspNetCoreServiceCollectionExtensions
{
    public static IServiceCollection AddConsulAspNetCore(this IServiceCollection services)
    {
        services.TryAddSingleton<DefaultServerAddressesProvider>();
        services.TryAddSingleton<IServerAddressesProvider>(sp => sp.GetRequiredService<DefaultServerAddressesProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ServerAddressesFilter>());

        return services;
    }
}
