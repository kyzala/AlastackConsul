namespace Alastack.Extensions.Configuration.Consul;

/// <summary>
/// The format of a Consul KV value.
/// </summary>
public enum ConsulConfigurationFormat
{
    /// <summary>
    /// Determines the format from the path extension.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json = 1,

    /// <summary>
    /// INI format.
    /// </summary>
    Ini = 2,

    /// <summary>
    /// XML format.
    /// </summary>
    Xml = 3
}
