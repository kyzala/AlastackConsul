using Alastack.Extensions.Configuration.Consul;

namespace Alastack.Consul;

/// <summary>
/// Maps <see cref="AgentOptions"/> to the settings used to identify a shared Consul client.
/// </summary>
internal static class ConsulClientSettingsExtensions
{
    internal static ConsulClientSettings ToConsulClientSettings(this AgentOptions agentOptions)
    {
        ArgumentNullException.ThrowIfNull(agentOptions);

        return new ConsulClientSettings
        {
            // Address is required by validation, but honour the Consul client default when it is missing.
            Address = agentOptions.Address ?? new Uri("http://localhost:8500"),
            Datacenter = agentOptions.Datacenter,
            Token = agentOptions.Token,
            WaitTime = agentOptions.WaitTime,
            HttpClientTimeout = agentOptions.HttpClientTimeout
        };
    }
}
