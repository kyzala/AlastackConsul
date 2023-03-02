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
    public static string BuildRegistrationId(this ServiceRegistration registration)
    {
        return $"{registration.Name}:{registration.Version}#{registration!.Address.Host}:{registration!.Address.Port}";
    }

    /// <summary>
    /// Build a health check id.
    /// </summary>
    /// <param name="registration"><see cref="ServiceRegistration"/></param>
    /// <returns>A new service health check id.</returns>
    public static string BuildHealthCheckId(this ServiceRegistration registration)
    {        
        return $"{registration.Name}_hk_{Guid.NewGuid():n}";
    }

    /// <summary>
    /// Build a health check name.
    /// </summary>
    /// <param name="registration"><see cref="ServiceRegistration"/></param>
    /// <returns>A new service health check name.</returns>
    public static string BuildHealthCheckName(this ServiceRegistration registration)
    {
        return $"{registration.Name}_hk";
    }

    //public static string NormalizeHealthCheckAddress(this ServiceRegistration registration) 
    //{
    //    if (Uri.IsWellFormedUriString(registration.HealthCheck.Health, UriKind.Relative)) 
    //    {
    //        return new Uri(registration.Address, registration.HealthCheck.Health).ToString();
    //    }
    //    return registration.HealthCheck.Health;
    //}
}
