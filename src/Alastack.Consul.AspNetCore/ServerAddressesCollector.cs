using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alastack.Consul.AspNetCore
{
    public class ServerAddressesCollector : IServerAddressesCollector
    {
        public bool Configuered { get; set; }

        public void AddServerAddress(string address)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
