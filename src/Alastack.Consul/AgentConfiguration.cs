namespace Alastack.Consul;

/// <summary>
/// Consul agent configuration definition.
/// </summary>
public sealed class AgentConfiguration
{
    /// <summary>
    /// Datacenter to provide with each request. If not provided, the default agent datacenter is used.
    /// </summary>
    public string Datacenter { get; init; } = default!;

    /// <summary>
    /// The Uri to connect to the Consul agent.
    /// </summary>
    public Uri Address { get; init; } = default!;

    /// <summary>
    /// Token is used to provide an ACL token which overrides the agent's default token. 
    /// This ACL token is used for every request by clients created using this configuration.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// WaitTime limits how long a Watch will block. If not provided, the agent default values will be used.
    /// </summary>
    public TimeSpan? WaitTime { get; init; }

}
