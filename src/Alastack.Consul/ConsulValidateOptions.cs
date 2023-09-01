using Microsoft.Extensions.Options;

namespace Alastack.Consul;

/// <summary>
/// Custom implementation for validating <see cref="ConsulOptions"/>.
/// </summary>
public class ConsulValidateOptions : IValidateOptions<ConsulOptions>
{
    /// <summary>
    ///  Invoked to validate a <see cref="ConsulOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConsulOptions"/> instance being validated.</param>
    /// <param name="options">The <see cref="ConsulOptions"/> instance to validate.</param>
    /// <returns>The <see cref="ValidateOptionsResult"/> result.</returns>
    public ValidateOptionsResult Validate(string? name, ConsulOptions options)
    {        
        if (options.Registration != null || options.Configuration != null)
        {
            if (options.Agent == null)
            {
                return ValidateOptionsResult.Fail("Agent is null.");
            }
            //return ValidateOptionsResult.Fail("Registration and Configuration are both null.");
        }

        return ValidateOptionsResult.Success;
    }
}
