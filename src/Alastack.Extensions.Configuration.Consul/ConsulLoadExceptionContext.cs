namespace Alastack.Extensions.Configuration.Consul;

/// <summary>
/// Context passed to <see cref="ConsulConfigurationSource.OnLoadException"/>.
/// </summary>
public sealed class ConsulLoadExceptionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulLoadExceptionContext"/> class.
    /// </summary>
    /// <param name="exception">The exception that occurred while loading the Consul key.</param>
    public ConsulLoadExceptionContext(Exception exception)
    {
        Exception = exception;
    }

    /// <summary>
    /// Gets the exception that occurred while loading the Consul key.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception should be ignored.
    /// </summary>
    public bool Ignore { get; set; }
}
