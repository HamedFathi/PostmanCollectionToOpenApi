namespace PostmanCollectionToOpenApi.Models;

public class JTokenDetail
{
    public bool IsObjectOrArray { get; set; }
    public JToken JToken { get; set; } = null!;
    public string JTokenString { get; set; } = null!;
    public JTokenType JTokenType { get; set; }
    public string? LastPathItem { get; set; }
    public string[]? Parents { get; set; }
    public string[] Path { get; set; } = null!;
    public string SharedKey { get; set; } = null!;
    public string SharedParentKey { get; set; } = null!;
    public object? Value { get; set; }
}