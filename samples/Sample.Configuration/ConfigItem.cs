namespace Sample.Configuration;
public record ConfigItem
{        
    public string Key { get; set; } = default!;

    public string Value { get; set; } = default!;

    public int? OrderId { get; set; }

    public bool? Optional { get; set; }

    public DateTime? Date { get; set; }

    public string[]? Tags { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }
}
