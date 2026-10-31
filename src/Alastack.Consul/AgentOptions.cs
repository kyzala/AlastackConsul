namespace Alastack.Consul;

/// <summary>
/// Consul agent options.
/// </summary>
public sealed class AgentOptions
{
    /// <summary>
    /// Datacenter to provide with each request. If not provided, the default agent datacenter is used.
    /// </summary>
    public string Datacenter { get; set; } = default!;

    /// <summary>
    /// The Uri to connect to the Consul agent.
    /// </summary>
    public Uri Address { get; set; } = default!;

    /// <summary>
    /// Token is used to provide an ACL token which overrides the agent's default token. 
    /// This ACL token is used for every request by clients created using this configuration.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// WaitTime limits how long a Watch will block. If not provided, the agent default values will be used.
    /// </summary>
    public TimeSpan? WaitTime { get; set; }

    /// <summary>
    /// The maximum time to wait for a Consul request to complete.
    /// If not provided, the Consul client default is used.
    /// </summary>
    public TimeSpan? HttpClientTimeout { get; set; }

}
