using Consul;
using Microsoft.Extensions.Configuration;

namespace Alastack.Extensions.Configuration.Consul;

/// <summary>
/// Represents a Consul KV value as an <see cref="IConfigurationSource"/>.
/// </summary>
public sealed class ConsulConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// Gets or sets the Consul KV key to load. The key is used as-is; it is not normalized.
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// Gets or sets the format of the Consul KV value. Defaults to <see cref="ConsulConfigurationFormat.Auto"/>,
    /// which determines the format from the <see cref="Path"/> extension.
    /// </summary>
    public ConsulConfigurationFormat Format { get; set; } = ConsulConfigurationFormat.Auto;

    /// <summary>
    /// Gets or sets a value indicating whether the Consul key is optional.
    /// When <c>false</c>, a missing key throws an exception.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is reloaded when the Consul key changes.
    /// </summary>
    public bool ReloadOnChange { get; set; }

    /// <summary>
    /// Gets or sets the maximum time a blocking query waits for changes. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan PollingWaitTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the factory used to create the <see cref="IConsulClient"/>.
    /// When not set, a client using the Consul default agent is created.
    /// </summary>
    public Func<IConsulClient>? ClientFactory { get; set; }

    /// <summary>
    /// Gets or sets the action used to handle exceptions that occur while loading the Consul key.
    /// </summary>
    public Action<ConsulLoadExceptionContext>? OnLoadException { get; set; }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new InvalidOperationException("The Consul Configuration Path is required.");
        }

        if (PollingWaitTime <= TimeSpan.Zero)
        {
            // A non-positive wait turns the blocking query into a non-blocking one, which makes
            // the watch loop poll the server as fast as the network allows.
            throw new InvalidOperationException("The Consul Configuration PollingWaitTime must be greater than zero.");
        }

        var format = Format == ConsulConfigurationFormat.Auto ? ResolveFormat(Path) : Format;
        return new ConsulConfigurationProvider(this, format);
    }

    internal static ConsulConfigurationFormat ResolveFormat(string path)
    {
        return System.IO.Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".json" => ConsulConfigurationFormat.Json,
            ".ini" => ConsulConfigurationFormat.Ini,
            ".xml" => ConsulConfigurationFormat.Xml,
            _ => throw new InvalidOperationException(
                $"Unable to determine the configuration format for '{path}'. " +
                "Specify ConsulConfigurationSource.Format explicitly. Supported extensions are .json, .ini and .xml.")
        };
    }
}
