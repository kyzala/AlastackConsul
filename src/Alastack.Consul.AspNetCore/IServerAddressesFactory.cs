namespace Alastack.Consul.AspNetCore;

public interface IServerAddressesFactory
{
    bool Configuered { get; set; }

    void AddServerAddress(string address);

    ICollection<string> GetServerAddresses();
}
