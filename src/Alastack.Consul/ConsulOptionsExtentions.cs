namespace Alastack.Consul;
public static class ConsulOptionsExtentions
{
    public static string BuildConfigurationPath(this AppConfig service)
    {
        if (service.Configuration!.Path == null) 
        {
            service.Configuration.Path = $"{service.Namespace}/{service.Name}:{service.Version}";
        }
        return service.Configuration.Path;
    }

    public static string BuildRegistrationName(this AppConfig service)
    {
        if (service.Registration!.Name == null) 
        {
            service.Registration!.Name = $"{service.Name}";//:{service.Version}";
        }
        return service.Registration!.Name;
    }

    public static string BuildRegistrationId(this AppConfig service)
    {
        if (service.Registration!.Id == null) 
        {
            service.Registration.Id = $"{service.Name}:{service.Version}#{service.Registration!.Address.Host}:{service.Registration!.Address.Port}";
        }
        return service.Registration.Id;
    }

    public static string BuildHealthCheckName(this AppConfig service)
    {
        if (service.Registration!.HealthCheck.Name == null)
        {
            var registrationName = service.BuildRegistrationName();
            service.Registration.HealthCheck.Name = $"{registrationName}_healthcheck";
        }
        return service.Registration.HealthCheck.Name;

    }

    public static string BuildHealthCheckAddress(this AppConfig service) 
    {
        if (Uri.IsWellFormedUriString(service.Registration!.HealthCheck.Health, UriKind.Relative)) 
        {
            service.Registration.HealthCheck.Health = new Uri(service.Registration.Address, service.Registration.HealthCheck.Health).ToString();
        }
        return service.Registration.HealthCheck.Health;
    }
}
