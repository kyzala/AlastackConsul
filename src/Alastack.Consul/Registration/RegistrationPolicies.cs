namespace Alastack.Consul.Registration;

internal static class RegistrationPolicies
{
    public const string RegistrationIdNullPolicy = nameof(RegistrationIdNullPolicy);

    //public const string HealthCheckIdPolicy = nameof(HealthCheckIdPolicy);

    //public const string HealthCheckNamePolicy = nameof(HealthCheckNamePolicy);
}

internal static class RegistrationIdNullPolicy
{
    public const string Default = nameof(Default);

    public const string Consul = nameof(Consul);

    public static bool IsConsul(string policyValue)
    {
        if (string.IsNullOrWhiteSpace(policyValue))
        {
            return false;
        }
        return string.Equals(Consul, policyValue, StringComparison.OrdinalIgnoreCase);
    }
}
