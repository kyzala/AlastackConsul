using Alastack.Consul;
using System.Reflection.Metadata.Ecma335;
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

        var consulOptions = configuration.GetSection(key).Get<ConsulOptions>();
        if (consulOptions?.Configuration == null) 
        {
            if (optional)
            {
                return builder;
            }
            else
            {
                throw new ArgumentNullException($"The consul configuration is null.");
            }
        }
        return builder.AddConsulConfiguration(consulOptions);
    }

    /// <summary>
    ///  Adds Consul as a configuration source to the <see cref="IConfigurationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="consulOptions">The <see cref="ConsulOptions" />.</param>
    /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
    public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, ConsulOptions consulOptions)
    {
        if (consulOptions == null)
        {
            throw new ArgumentNullException(nameof(ConsulOptions));
        }
        if (consulOptions.Configuration?.Sets != null)
        {
            var configuration = consulOptions.Configuration;
            var pathPrefix = $"{configuration.PathBase.TrimStart('/')}/{configuration.Namespace}";
            foreach (var configurationSet in configuration.Sets)
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
                            config.Address = consulOptions.Agent.Address;
                            config.Token = consulOptions.Agent.Token;
                            config.Datacenter = consulOptions.Agent.Datacenter;
                            config.WaitTime = consulOptions.Agent.WaitTime;
                        };
                    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = configurationSet.IgnoreException; };
                });
            }
        }

        return builder;
    }
}
