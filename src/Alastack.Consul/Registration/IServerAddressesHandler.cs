namespace Alastack.Consul.Registration;

public interface IServerAddressesHandler
{
    bool Configured { get; set; }

    void AddServerAddress(string address);

    Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken);
}
