using Alastack.Consul.Registration;
using Microsoft.Extensions.Options;

namespace Alastack.Consul;

/// <summary>
/// Used to setup defaults for the <see cref="ConsulOptions"/>.
/// </summary>
public class ConsulPostConfigureOptions : IPostConfigureOptions<ConsulOptions>
{
    /// <summary>
    /// Invoked to post configure a <see cref="ConsulOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConsulOptions"/> instance being configured.</param>
    /// <param name="options">The <see cref="ConsulOptions"/> instance to configure.</param>
    public void PostConfigure(string? name, ConsulOptions options)
    {       
        if (options.Registration?.Instances != null) 
        {
            foreach (var instance in options.Registration.Instances) 
            {
                instance.Configure(registration: options.Registration);
            }
        }
    }
}
