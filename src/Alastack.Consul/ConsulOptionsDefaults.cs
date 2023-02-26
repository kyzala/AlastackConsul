namespace Alastack.Consul;
public static class ConsulOptionsDefaults
{
    public static class Service
    {
        public const string Environment = "Production";

        public const string Namespace = "default";

        public const string Version = "1.0.0";
    }

    public static class Configuration
    {
        public const string Manifest = "manifest.json";

        public static readonly TimeSpan PollingWaitTime = TimeSpan.FromSeconds(5);
    }

    public static class Registry
    {
        public static readonly TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1);

        public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

        public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
    }
}
