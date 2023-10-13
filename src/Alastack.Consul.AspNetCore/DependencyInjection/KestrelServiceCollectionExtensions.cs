using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KestrelServiceCollectionExtensions
{
    public static IServiceCollection AddAspNetCore(this IServiceCollection services)
    {        
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IServerAddressesHandler, KestrelServerAddressesHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ServerAddressesFilter>());

        return services;
    }
}
