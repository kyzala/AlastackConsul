using Microsoft.Extensions.DependencyInjection;

namespace Alastack.Consul;

/// <summary>
/// An interface for configuring Naming services.
/// </summary>
public interface INamingBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>. where Naming services are configured.
    /// </summary>
    IServiceCollection Services { get; }
}
