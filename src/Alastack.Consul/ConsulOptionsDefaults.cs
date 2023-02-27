namespace Alastack.Consul;
public static class ConsulOptionsDefaults
{
    public static class Service
    {
        public const string Environment = "Production";

        public const string Namespace = "default";

        public const string Version = "1.0.0";
    }

    

    
}

public static class ConsulRegistrationDefaults
{
    public static readonly TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1);

    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
}

public static class ConsulConfigurationDefaults
{
    public const string Namespace = "ns_default";

    public const string Group = "gp_default";

    public const bool IgnoreException = true;

    public static readonly TimeSpan PollingWaitTime = TimeSpan.FromSeconds(5);

}
