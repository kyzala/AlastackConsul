namespace Alastack.Consul;

/// <summary>
/// Application configuration set definition.
/// </summary>
public sealed class ConfigurationSet
{
    /// <summary>
    /// The configuration set id.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Group for the configuration set. Defaults to <c>gp.default</c>.
    /// </summary>
    public string Group { get; set; } = AppConfigurationDefaults.Group;

    /// <summary>
    /// The configuration set description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the configuration set is optional. Defaults to <c>false</c>.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Whether the configuration should be reloaded if the configuration set changes.  Defaults to <c>false</c>.
    /// </summary>
    public bool ReloadOnChange { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount of time to wait for changes to a set id if <see cref="ConfigurationSet.ReloadOnChange"/> is true.
    /// </summary>
    public TimeSpan PollingWaitTime { get; set; } = AppConfigurationDefaults.PollingWaitTime;

    /// <summary>
    /// Indicating whether the exception should be ignored. Set to true to prevent the exception from being thrown. Defaults to <c>false</c>.
    /// </summary>
    public bool IgnoreException { get; set; }
}
