namespace Alastack.Consul;
public static class ConsulOptionsExtentions
{
    public static string BuildRegistrationName(this ServiceRegistration registration)
    {
        if (registration.Name == null) 
        {
            registration.Name = $"{registration.Name}";//:{service.Version}";
        }
        return registration.Name;
    }

    public static string BuildRegistrationId(this ServiceRegistration registration)
    {
        registration.Id ??= $"{registration.Name}:{registration.Version}#{registration!.Address.Host}:{registration!.Address.Port}";
        return registration.Id;
    }

    public static string BuildHealthCheckName(this ServiceRegistration registration)
    {
        if (registration!.HealthCheck.Name == null)
        {
            var registrationName = registration.BuildRegistrationName();
            registration.HealthCheck.Name = $"{registrationName}_healthcheck";
        }
        return registration.HealthCheck.Name;

    }

    public static string BuildHealthCheckAddress(this ServiceRegistration registration) 
    {
        if (Uri.IsWellFormedUriString(registration!.HealthCheck.Health, UriKind.Relative)) 
        {
            registration.HealthCheck.Health = new Uri(registration.Address, registration.HealthCheck.Health).ToString();
        }
        return registration.HealthCheck.Health;
    }
}
