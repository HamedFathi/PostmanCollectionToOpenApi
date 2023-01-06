namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanBodyExtensions
{
    public static IDictionary<string, OpenApiMediaType> ToContent(this PostmanBody postmanBody)
    {
        var result = new Dictionary<string, OpenApiMediaType>();

        return result;
    }
}