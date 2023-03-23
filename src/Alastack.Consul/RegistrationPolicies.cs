using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alastack.Consul;

internal static class RegistrationPolicies
{
    public const string RegistrationIdPolicy = nameof(RegistrationIdPolicy);

    public const string HealthCheckIdPolicy = nameof(HealthCheckIdPolicy);

    public const string HealthCheckNamePolicy = nameof(HealthCheckNamePolicy);
}

internal static class RegistrationPolicyValues
{
    public const string Default = "Default";

    public const string ByConsul = "ByConsul";

    public static bool IsByConsul(string policyValue)
    {
        if (string.IsNullOrWhiteSpace(policyValue))
        {
            return false;
        }
        return String.Equals(ByConsul, policyValue, StringComparison.OrdinalIgnoreCase);
    }
}
