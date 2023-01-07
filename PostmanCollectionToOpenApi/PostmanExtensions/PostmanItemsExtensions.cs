#nullable enable

using PostmanCollectionToOpenApi.OpenApiExtensions;

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanItemsExtensions
{
    public static IList<OpenApiParameter> GetOpenApiParameters(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        var result = new List<OpenApiParameter>();
        if (postmanItems.Request?.RequestClass?.Header?.HeaderArray != null)
        {
            var cookies = postmanItems.Request?.RequestClass?.Header?.HeaderArray?
                .Where(x => string.Equals(x.Key, "cookie", StringComparison.OrdinalIgnoreCase) && x.Disabled != true);
            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    result.Add(new OpenApiParameter
                    {
                        In = ParameterLocation.Cookie,
                        Name = cookie.Key,
                        Description = cookie.Description?.String,
                        Example = cookie.Value?.ToExample(variables),
                        Schema = cookie.Value?.ToOpenApiSchema(variables)
                    });
                }
            }
        }

        var queries = postmanItems.Request?.RequestClass?.Url?.UrlClass?.Query;
        if (queries != null)
        {
            foreach (var query in queries)
            {
                if (query.Disabled == true) continue;
                result.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = query.Key,
                    Description = query.Description?.String,
                    Example = query.Value?.ToExample(variables),
                    Schema = query.Value?.ToOpenApiSchema(variables)
                });
            }
        }

        if (postmanItems.Request?.RequestClass?.Header?.HeaderArray != null)
        {
            var headers = postmanItems.Request?.RequestClass?.Header?.HeaderArray?
                .Where(x => !string.Equals(x.Key, "cookie", StringComparison.OrdinalIgnoreCase) && x.Disabled != true);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    result.Add(new OpenApiParameter
                    {
                        In = ParameterLocation.Header,
                        Name = header.Key,
                        Description = header.Description?.String,
                        Example = header.Value?.ToExample(variables),
                        Schema = header.Value?.ToOpenApiSchema(variables)
                    });
                }
            }
        }

        // ParameterLocation.Path
        // Query string?

        return result;
    }

    public static OpenApiRequestBody GetOpenApiRequestBody(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        if (postmanItems.Request == null) throw new ArgumentNullException(nameof(postmanItems));

        return new OpenApiRequestBody
        {
            Description = postmanItems.Request?.RequestClass?.Description?.String,
            Content = postmanItems.Request?.RequestClass?.ToContent(variables)
        };
    }

    public static OpenApiResponses GetOpenApiResponses(this PostmanItems postmanItems, Dictionary<string, string>? variables)
    {
        if (postmanItems == null) throw new ArgumentNullException(nameof(postmanItems));
        if (postmanItems.Response == null) throw new ArgumentNullException(nameof(postmanItems));

        var result = new OpenApiResponses();
        return postmanItems.Response.Count > 0 ? postmanItems.Response.ToOpenApiResponses(variables) : result;
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