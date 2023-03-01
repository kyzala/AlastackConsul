namespace Alastack.Consul;
public sealed class AppConfiguration
{
    public string PathBase { get; set; } = default!;

    public string Namespace { get; set; } = ConsulConfigurationDefaults.Namespace;

    public IList<ConfigurationSet>? Sets { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }
}
