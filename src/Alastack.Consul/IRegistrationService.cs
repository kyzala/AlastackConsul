using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alastack.Consul
{
    public interface IRegistrationService
    {
        Task ServiceRegister(CancellationToken cancellationToken);

        Task ServiceDeregister(CancellationToken cancellationToken);
    }
}
