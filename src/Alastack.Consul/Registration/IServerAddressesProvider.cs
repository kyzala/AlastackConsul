namespace Alastack.Consul.Registration;

/// <summary>
/// Provides the listen addresses of the hosting application server.
/// </summary>
public interface IServerAddressesProvider
{
    /// <summary>
    /// Gets the server listen addresses.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server listen addresses.</returns>
    Task<IReadOnlyCollection<string>> GetServerAddressesAsync(CancellationToken cancellationToken);
}
