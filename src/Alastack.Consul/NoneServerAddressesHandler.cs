using System.Collections.ObjectModel;

namespace Alastack.Consul;

public class NoneServerAddressesHandler : IServerAddressesHandler
{
    private readonly ICollection<string> _addresses = new Collection<string>();

    public bool Configuered { get; set; } = true;        

    public void AddServerAddress(string address)
    {
        _addresses.Add(address);
    }

    public async Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken)
    {
        return await Task.FromResult(_addresses);
    }
}
