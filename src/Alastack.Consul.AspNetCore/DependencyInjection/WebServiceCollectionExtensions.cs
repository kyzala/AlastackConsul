using Alastack.Consul.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebServiceCollectionExtensions
{
    private static IServiceCollection AddConsulCore(this IServiceCollection services)
    {
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ConsulOptions>, ConsulPostConfigureOptions>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ConsulOptions>, ConsulValidateOptions>());
        //services.AddHostedService<ServiceRegistrationService>();

        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ServerAddressesFilter>());
        services.AddHostedService<WebRegistrationHostedService>();

        return services;
    }
}
