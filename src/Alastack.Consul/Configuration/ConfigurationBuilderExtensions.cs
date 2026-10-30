using Alastack.Consul;
using Alastack.Consul.Configuration;
using Winton.Extensions.Configuration.Consul;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods to add consul configuration.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///  Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="key">The Consul configuration key. Defaults to <c>Consul</c>.</param>
    /// <param name="optional">Whether the configuration is optional.</param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, string key = "Consul", bool optional = false)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddConsulConfiguration(builder.Build(), key, optional);
        return builder;
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
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, IConfiguration configuration, string key = "Consul", bool optional = false)
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
        return builder.AddConsulConfiguration(configurationOptions);
    }

    /// <summary>
    ///  Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="configurationOptions">The <see cref="ConsulConfigurationOptions" />.</param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, ConsulConfigurationOptions configurationOptions)
    {
        if (configurationOptions == null)
        {
            throw new ArgumentNullException(nameof(configurationOptions));
        }
        if (configurationOptions.Sets is { Count: > 0 })
        {
            if (string.IsNullOrWhiteSpace(configurationOptions.PathBase))
            {
                throw new InvalidOperationException("The Consul Configuration.PathBase is required when Configuration.Sets is specified.");
            }
            if (configurationOptions.Agent?.Address == null)
            {
                throw new InvalidOperationException("The Consul Agent configuration is required when Configuration.Sets is specified.");
            }

            var pathPrefix = $"{configurationOptions.PathBase.TrimStart('/')}/{configurationOptions.Namespace}";
            foreach (var configurationSet in configurationOptions.Sets)
            {
                var pathKey = $"{pathPrefix}/{configurationSet.Group}/{configurationSet.Id}";
                builder.AddConsul(pathKey, options =>
                {
                    options.Optional = configurationSet.Optional;
                    options.ReloadOnChange = configurationSet.ReloadOnChange;
                    options.PollWaitTime = configurationSet.PollingWaitTime;
                    options.ConsulConfigurationOptions =
                        config =>
                        {
                            config.Address = configurationOptions.Agent.Address;
                            config.Token = configurationOptions.Agent.Token;
                            config.Datacenter = configurationOptions.Agent.Datacenter;
                            config.WaitTime = configurationOptions.Agent.WaitTime;
                        };
                    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = configurationSet.IgnoreException; };
                });
            }
        }

        return builder;
    }
}
