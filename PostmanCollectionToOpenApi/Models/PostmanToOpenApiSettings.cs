// ReSharper disable UnusedMember.Global

namespace PostmanCollectionToOpenApi.Models;

public class PostmanToOpenApiSettings
{
    public PostmanToOpenApiSettings()
    {
        DefaultVersion = "0.0.1";
        PostmanVariables = new List<PostmanEnvironmentVariable>();
    }

    public string DefaultVersion { get; set; }
    public List<PostmanEnvironmentVariable> PostmanVariables { get; set; }
}