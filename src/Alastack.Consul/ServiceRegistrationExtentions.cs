namespace Alastack.Consul;

/// <summary>
/// ServiceRegistration Extentions.
/// </summary>
public static class ServiceRegistrationExtentions
{
    /// <summary>
    /// Build a service registration id.
    /// </summary>
    /// <param name="registration"><see cref="ServiceRegistration"/></param>
    /// <returns>A new service registration id.</returns>
    public static string? BuildRegistrationId(this ServiceRegistration registration)
    {
        if (registration.IsPolicyDefault(RegistrationPolicies.RegistrationIdNullPolicy))
        {
            return $"{registration.Name}@{registration.Address.Host}:{registration.Address.Port}";
        }
        return null;
        
    }

    /*
    /// <summary>
    /// Build a health check id.
    /// </summary>
    /// <param name="registration"><see cref="ServiceRegistration"/></param>
    /// <returns>A new service health check id.</returns>
    public static string? BuildHealthCheckId(this ServiceRegistration registration)
    {
        if (registration.IsPolicyDefault(RegistrationPolicies.HealthCheckIdPolicy)) 
        {
            return $"{registration.Name}_hk_{Guid.NewGuid():n}";
        }
        return null;
    }
    

    /// <summary>
    /// Build a health check name.
    /// </summary>
    /// <param name="registration"><see cref="ServiceRegistration"/></param>
    /// <returns>A new service health check name.</returns>
    public static string? BuildHealthCheckName(this ServiceRegistration registration)
    {
        if (registration.IsPolicyDefault(RegistrationPolicies.HealthCheckNamePolicy)) 
        {
            return $"{registration.Name}_hk";
        }
        return null;
    }
    */

    //public static string NormalizeHealthCheckAddress(this ServiceRegistration registration) 
    //{
    //    if (Uri.IsWellFormedUriString(registration.HealthCheck.Health, UriKind.Relative)) 
    //    {
    //        return new Uri(registration.Address, registration.HealthCheck.Health).ToString();
    //    }
    //    return registration.HealthCheck.Health;
    //}

    private static bool IsPolicyDefault(this ServiceRegistration registration, string policy) 
    {
        var policyValue = registration.GetPolicyValue(policy);

        return String.Equals(RegistrationIdNullPolicy.Default, policyValue, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPolicyValue(this ServiceRegistration registration, string policy)
    {
        if (registration.Metadata != null && registration.Metadata.TryGetValue(policy, out var value))
        {
            if (RegistrationIdNullPolicy.IsConsul(value))
            {
                return RegistrationIdNullPolicy.Consul;
            }
        }
        return RegistrationIdNullPolicy.Default;
    }
}
