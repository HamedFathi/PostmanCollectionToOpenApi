using PostmanCollectionToOpenApi.OpenApiExtensions;

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanItemsExtensions
{
    public static IList<OpenApiParameter> GetOpenApiParameters(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        var result = new List<OpenApiParameter>();

        var queries = postmanItems.Request?.RequestClass.Url?.UrlClass.Query;
        if (queries != null)
        {
            foreach (var query in queries)
            {
                if (query.Disabled == true) continue;
                result.Add(new OpenApiParameter()
                {
                    In = ParameterLocation.Query,
                    Name = query.Key,
                    Description = query.Description?.String,
                    Example = query.Value.ToExample(variables),
                    Schema = query.Value.ToOpenApiSchema(variables)
                });
            }
        }

        return result;
    }

    public static OpenApiRequestBody GetOpenApiRequestBody(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        if (postmanItems.Request == null) throw new ArgumentNullException(nameof(postmanItems));

        var rawValue = postmanItems.Request?.RequestClass.Body?.Raw;
        if (rawValue != null)
        {
            var schema = rawValue.ToOpenApiSchema(variables);
            var example = rawValue.ToExample(variables);
        }

        var result = new OpenApiRequestBody
        {
            Description = postmanItems.Request?.RequestClass.Description?.String,
            Content = postmanItems.Request?.RequestClass.Body?.ToContent()
        };

        return result;
    }

    public static OpenApiResponses GetOpenApiResponses(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        if (postmanItems.Response == null) throw new ArgumentNullException(nameof(postmanItems));

        var result = new OpenApiResponses();

        return result;
    }

    public static Dictionary<string, List<PostmanItems>> GetRequestPathKeys(this IEnumerable<PostmanItems> postmanItems, Dictionary<string, string>? variables)
    {
        var result = new Dictionary<string, List<PostmanItems>>();
        foreach (var val in postmanItems)
        {
            var key = val.Request.GetPath()?.ReplaceVariables(variables);
            if (key != null && !string.IsNullOrWhiteSpace(key))
            {
                if (result.ContainsKey(key))
                    result[key].Add(val);
                else
                    result.Add(key, new List<PostmanItems> { val });
            }
        }

        return result;
    }
}