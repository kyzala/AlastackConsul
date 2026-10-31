using Alastack.Consul;
using Alastack.Consul.Configuration;
using Alastack.Extensions.Configuration.Consul;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods to add consul configuration.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///  Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// </summary>
    /// <remarks>
    /// The current configuration is built immediately to read the settings, which loads every source
    /// already added to <paramref name="builder"/>. Prefer the overload that accepts an existing
    /// <see cref="IConfiguration"/> and call this overload before adding sources that are expensive
    /// to load (for example Consul itself).
    /// </remarks>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <param name="optional">Whether the configuration is optional.</param>
    /// <param name="clientFactory">
    /// The factory used to create Consul clients. When not provided, <see cref="ConsulClientFactory.Default"/> is used.
    /// </param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(
        this IConfigurationBuilder builder,
        string key = "Consul",
        bool optional = false,
        IConsulClientFactory? clientFactory = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // The current configuration is only read, so release the snapshot root instead of keeping it alive.
        var configuration = builder.Build();
        try
        {
            return builder.AddConsulConfiguration(configuration, key, optional, clientFactory);
        }
        finally
        {
            (configuration as IDisposable)?.Dispose();
        }
    }

    /// <summary>
    /// Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// The <c>{key}:Configuration</c> section binds to the configuration settings and the shared
    /// <c>{key}:Agent</c> section binds to <see cref="ConsulConfigurationOptions.Agent"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <param name="optional">Whether the configuration is optional.</param>
    /// <param name="clientFactory">
    /// The factory used to create Consul clients. When not provided, <see cref="ConsulClientFactory.Default"/> is used.
    /// </param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(
        this IConfigurationBuilder builder,
        IConfiguration configuration,
        string key = "Consul",
        bool optional = false,
        IConsulClientFactory? clientFactory = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var root = configuration.GetSection(key);
        var configurationOptions = root.GetSection("Configuration").Get<ConsulConfigurationOptions>();
        if (configurationOptions == null)
        {
            if (optional)
            {
                return builder;
            }
            else
            {
                throw new InvalidOperationException($"The Consul configuration section '{key}:Configuration' was not found.");
            }
        }
        root.GetSection("Agent").Bind(configurationOptions.Agent);
        if (configurationOptions.Sets is not { Count: > 0 })
        {
            // The section exists but configures nothing: reporting success for a required
            // configuration would hide the misconfiguration until the first missing value shows up.
            if (optional)
            {
                return builder;
            }

            throw new InvalidOperationException(
                $"The Consul configuration section '{key}:Configuration:Sets' must contain at least one set unless the configuration is optional.");
        }

        return builder.AddConsulConfiguration(configurationOptions, clientFactory);
    }

    /// <summary>
    ///  Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="configurationOptions">The <see cref="ConsulConfigurationOptions" />.</param>
    /// <param name="clientFactory">
    /// The factory used to create Consul clients. When not provided, <see cref="ConsulClientFactory.Default"/> is used,
    /// which shares one client across all sources with the same agent settings.
    /// </param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(
        this IConfigurationBuilder builder,
        ConsulConfigurationOptions configurationOptions,
        IConsulClientFactory? clientFactory = null)
    {
        if (configurationOptions == null)
        {
            throw new ArgumentNullException(nameof(configurationOptions));
        }

        if (configurationOptions.Sets is not { Count: > 0 })
        {
            return builder;
        }

        if (string.IsNullOrWhiteSpace(configurationOptions.PathBase))
        {
            throw new InvalidOperationException("The Consul Configuration.PathBase is required when Configuration.Sets is specified.");
        }
        if (configurationOptions.Agent?.Address == null)
        {
            throw new InvalidOperationException("The Consul Agent configuration is required when Configuration.Sets is specified.");
        }

        var factory = clientFactory ?? ConsulClientFactory.Default;
        var clientSettings = configurationOptions.Agent.ToConsulClientSettings();

        var pathPrefix = BuildKey(
            configurationOptions.PathBase,
            string.IsNullOrWhiteSpace(configurationOptions.Namespace)
                ? ConfigurationDefaults.Namespace
                : configurationOptions.Namespace);

        foreach (var configurationSet in configurationOptions.Sets)
        {
            if (string.IsNullOrWhiteSpace(configurationSet.Id))
            {
                throw new InvalidOperationException("The Consul Configuration.Sets[].Id is required when Configuration.Sets is specified.");
            }

            if (configurationSet.PollingWaitTime <= TimeSpan.Zero)
            {
                // A non-positive wait turns the blocking query into a non-blocking one.
                throw new InvalidOperationException("The Consul Configuration.Sets[].PollingWaitTime must be greater than zero.");
            }

            var group = string.IsNullOrWhiteSpace(configurationSet.Group)
                ? ConfigurationDefaults.Group
                : configurationSet.Group;
            var pathKey = BuildKey(pathPrefix, group, configurationSet.Id);

            if (configurationSet.Format == ConsulConfigurationFormat.Auto)
            {
                // Resolve the format while reading the configuration, so an unsupported Id extension
                // fails here instead of later when the configuration root is built.
                ConsulConfigurationSource.ResolveFormat(pathKey);
            }

            // Calling this method twice with the same key must not add a second provider (and watch).
            if (builder.Sources.OfType<ConsulConfigurationSource>().Any(source => source.Path == pathKey))
            {
                continue;
            }

            builder.AddConsul(pathKey, source =>
            {
                source.Optional = configurationSet.Optional;
                source.ReloadOnChange = configurationSet.ReloadOnChange;
                source.PollingWaitTime = configurationSet.PollingWaitTime;
                source.Format = configurationSet.Format;
                source.ClientFactory = () => factory.GetClient(clientSettings);
                source.OnLoadException = exceptionContext => { exceptionContext.Ignore = configurationSet.IgnoreException; };
            });
        }

        return builder;
    }

    private static string BuildKey(params string?[] segments)
    {
        return string.Join(
            '/',
            segments
                .Select(segment => segment?.Trim().Trim('/'))
                .Where(segment => !string.IsNullOrEmpty(segment)));
    }
}
