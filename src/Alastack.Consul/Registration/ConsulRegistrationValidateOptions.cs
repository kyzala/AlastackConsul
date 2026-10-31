using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

/// <summary>
/// Custom implementation for validating <see cref="ConsulRegistrationOptions"/>.
/// </summary>
public class ConsulRegistrationValidateOptions : IValidateOptions<ConsulRegistrationOptions>
{
    /// <summary>
    /// Invoked to validate a <see cref="ConsulRegistrationOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConsulRegistrationOptions"/> instance being validated.</param>
    /// <param name="options">The <see cref="ConsulRegistrationOptions"/> instance to validate.</param>
    /// <returns>The <see cref="ValidateOptionsResult"/> result.</returns>
    public ValidateOptionsResult Validate(string? name, ConsulRegistrationOptions options)
    {
        if (options.Agent?.Address == null)
        {
            return ValidateOptionsResult.Fail("Agent.Address is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            return ValidateOptionsResult.Fail("Name is required.");
        }

        if (options.Agent.HttpClientTimeout is { } httpClientTimeout && httpClientTimeout <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail("Agent.HttpClientTimeout must be greater than zero.");
        }

        foreach (var instance in options.Instances)
        {
            if (instance.Address == null)
            {
                return ValidateOptionsResult.Fail("Instances.Address is required.");
            }
        }

        if (options.DefaultHealthCheck?.Enabled == true && options.DefaultHealthCheck.Endpoint == null)
        {
            return ValidateOptionsResult.Fail("DefaultHealthCheck.Endpoint is required when the health check is enabled.");
        }

        return ValidateOptionsResult.Success;
    }
}
