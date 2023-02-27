namespace Alastack.Consul;
public sealed class AppConfig
{
    public string Name { get; init; } = default!;

    public string Namespace { get; init; } = default!;

    public string? Group { get; set; }

    public string Version { get; init; } = default!;

    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public ConsulConfiguration? Configuration { get; init; }

    public ServiceRegistration? Registration { get; init; }
}
