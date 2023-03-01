namespace Alastack.Consul;

public static class ConsulRegistrationDefaults
{
    public static readonly TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1);

    public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
}

public static class ConsulConfigurationDefaults
{
    public const string Namespace = "ns.default";

    public const string Group = "gp.default";

    public const bool Optional = true;

    public const bool ReloadOnChange = true;

    public const bool IgnoreException = false;

    public static readonly TimeSpan PollingWaitTime = TimeSpan.FromSeconds(5);

}
