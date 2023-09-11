namespace Alastack.Consul;

public interface IServerAddressesHandler
{
    bool Configuered { get; set; }

    void AddServerAddress(string address);

    Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken);
}
