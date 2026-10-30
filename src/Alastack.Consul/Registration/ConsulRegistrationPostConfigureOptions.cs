using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

/// <summary>
/// Used to setup defaults for the <see cref="ConsulRegistrationOptions"/>.
/// </summary>
public class ConsulRegistrationPostConfigureOptions : IPostConfigureOptions<ConsulRegistrationOptions>
{
    /// <summary>
    /// Invoked to post configure a <see cref="ConsulRegistrationOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConsulRegistrationOptions"/> instance being configured.</param>
    /// <param name="options">The <see cref="ConsulRegistrationOptions"/> instance to configure.</param>
    public void PostConfigure(string? name, ConsulRegistrationOptions options)
    {
        foreach (var instance in options.Instances)
        {
            instance.Configure(registration: options);
        }
    }
}
