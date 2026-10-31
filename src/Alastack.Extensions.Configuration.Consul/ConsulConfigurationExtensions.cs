using Alastack.Extensions.Configuration.Consul;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods to add a Consul KV value as a configuration source.
/// </summary>
public static class ConsulConfigurationExtensions
{
    /// <summary>
    /// Adds a Consul KV value as a configuration source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">The Consul KV key to load.</param>
    /// <param name="configure">Used to configure the <see cref="ConsulConfigurationSource"/>.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddConsul(
        this IConfigurationBuilder builder,
        string path,
        Action<ConsulConfigurationSource>? configure = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var source = new ConsulConfigurationSource { Path = path };
        configure?.Invoke(source);
        return builder.Add(source);
    }
}
