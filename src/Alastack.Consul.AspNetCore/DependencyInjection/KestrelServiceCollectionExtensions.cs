using Alastack.Consul;
using Alastack.Consul.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KestrelServiceCollectionExtensions
{
    public static IServiceCollection AddAspNetCore(this IServiceCollection services)
    {        
        //services.AddSingleton<IServerAddressesHandler, ServerAddressesHandler>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IServerAddressesHandler, KestrelServerAddressesHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ServerAddressesFilter>());
        
        //var defaultRegistrationHostedService = services.FirstOrDefault(sd => 
        //sd.ServiceType ==  typeof(IHostedService) && sd.ImplementationType == typeof(RegistrationHostedService));

        //if (defaultRegistrationHostedService != null) 
        //{
        //    services.Remove(defaultRegistrationHostedService);
        //}

        //services.AddHostedService<RegistrationHostedService>();

        return services;
    }
}
