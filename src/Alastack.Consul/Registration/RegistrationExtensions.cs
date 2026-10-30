using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

/// <summary>
/// Registration extensions.
/// </summary>
public static class RegistrationExtensions
{

    public static void Configure(this ServiceInstance instance, ConsulRegistrationOptions registration) 
    {
        if (String.IsNullOrWhiteSpace(instance.Id))
        {
            instance.Id = instance.BuildServiceInstanceId(registration.Name, registration.Metadata);
        }

        if (instance.HealthCheck == null)
        {
            instance.HealthCheck = new HealthCheckOptions 
            {
                CheckId = registration.DefaultHealthCheck.CheckId,
                Name = registration.DefaultHealthCheck.Name,
                DeregisterCriticalServiceAfter = registration.DefaultHealthCheck.DeregisterCriticalServiceAfter,
                Interval = registration.DefaultHealthCheck.Interval,
                Endpoint = registration.DefaultHealthCheck.Endpoint,
                Timeout = registration.DefaultHealthCheck.Timeout
            };
        }

        if (String.IsNullOrWhiteSpace(instance.HealthCheck.Name))
        {
            instance.HealthCheck.Name = $"Service '{instance.Id}' check";
        }

        if (String.IsNullOrWhiteSpace(instance.HealthCheck.CheckId))
        {
            instance.HealthCheck.CheckId = $"service:{instance.Id}";
        }

        if (!instance.HealthCheck.Endpoint.IsAbsoluteUri)
        {
            instance.HealthCheck.Endpoint = new Uri(instance.Address, instance.HealthCheck.Endpoint);
        }
    }


    public static string? BuildServiceInstanceId(this ServiceInstance instance, string serviceName, IDictionary<string, string>? metadata)
    {
        if (metadata.IsPolicyDefault(RegistrationPolicies.RegistrationIdNullPolicy))
        {
            return $"{serviceName}@{instance.Address.Host}:{instance.Address.Port}";
        }
        return null;

    }

    private static bool IsPolicyDefault(this IDictionary<string, string>? metadata, string policy)
    {
        var policyValue = metadata.GetPolicyValue(policy);

        return string.Equals(RegistrationIdNullPolicy.Default, policyValue, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPolicyValue(this IDictionary<string, string>? metadata, string policy)
    {
        if (metadata != null && metadata.TryGetValue(policy, out var value))
        {
            if (RegistrationIdNullPolicy.IsConsul(value))
            {
                return RegistrationIdNullPolicy.Consul;
            }
        }
        return RegistrationIdNullPolicy.Default;
    }
}
