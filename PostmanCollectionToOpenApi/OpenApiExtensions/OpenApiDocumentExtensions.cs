// ReSharper disable UnusedMember.Global

namespace PostmanCollectionToOpenApi.OpenApiExtensions;

public static class OpenApiDocumentExtensions
{
    public static string ToOpenApiFile(this OpenApiDocument openApiDocument, OpenApiWriterType openApiWriterType = OpenApiWriterType.Json)
    {
        using var outputString = new StringWriter();
        if (openApiWriterType == OpenApiWriterType.Json)
        {
            var jsonWriter = new OpenApiJsonWriter(outputString);
            openApiDocument.SerializeAsV3(jsonWriter);
            return outputString.ToString();
        }
        var yamlWriter = new OpenApiYamlWriter(outputString);
        openApiDocument.SerializeAsV3(yamlWriter);
        return outputString.ToString();
    }
}