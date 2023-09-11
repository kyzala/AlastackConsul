using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alastack.Consul.AspNetCore
{
    public interface IServerAddressesCollector
    {
        bool Configuered { get; set; }

        void AddServerAddress(string address);

        Task<ICollection<string>> GetServerAddresses(CancellationToken cancellationToken);

    }
}
