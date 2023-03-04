namespace Sample.Common;
public record ConfigOptions
{
    public string Name { get; set; } = default!;

    public string Version { get; set; } = "1.0.0";

    public string? Description { get; set; }

    public IList<ConfigItem> Items { get; set; } = default!;
}