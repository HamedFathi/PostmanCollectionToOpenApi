using PostmanCollectionToOpenApi.OpenApiExtensions;

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanRequestClassExtensions
{
    public static IDictionary<string, OpenApiMediaType> ToContent(this PostmanRequestClass requestClass, Dictionary<string, string>? variables)
    {
        if (requestClass.Body != null)
        {
            if (requestClass.Body.Raw != null)
            {
                return requestClass.ConvertRaw(variables);
            }

            if (requestClass.Body.Urlencoded != null)
            {
                return requestClass.ConvertUrlEncoded(variables);
            }
        }
        return new Dictionary<string, OpenApiMediaType>();
    }

    private static IDictionary<string, OpenApiMediaType> ConvertRaw(this PostmanRequestClass requestClass, Dictionary<string, string>? variables)
    {
        var result = new Dictionary<string, OpenApiMediaType>();

        var raw = requestClass.Body?.Raw?.ReplaceVariables(variables);
        var schema = raw?.ToOpenApiSchema(variables);
        var example = raw?.ToExample(variables);

        if (requestClass.Header?.HeaderArray != null)
        {
            var contentType = requestClass.Header?.HeaderArray
                    .FirstOrDefault(x => string.Equals(x.Key, "content-type", StringComparison.OrdinalIgnoreCase) && x.Disabled != true)
                    ?.Value
                ;

            contentType ??= "application/json";

            result.Add(contentType, new OpenApiMediaType
            {
                Schema = schema,
                Example = example
            });
        }

        return result;
    }

    private static IDictionary<string, OpenApiMediaType> ConvertUrlEncoded(this PostmanRequestClass requestClass, Dictionary<string, string>? variables)
    {
        var result = new Dictionary<string, OpenApiMediaType>();

        var jsonText = requestClass.Body?.Urlencoded?.ToJsonString(variables);
        var schema = jsonText?.ToOpenApiSchema(variables);
        var example = jsonText?.ToExample(variables);

        if (requestClass.Header?.HeaderArray != null)
        {
            var contentType = requestClass.Header?.HeaderArray?
                    .FirstOrDefault(x => string.Equals(x.Key, "content-type", StringComparison.OrdinalIgnoreCase) && x.Disabled != true)
                    ?.Value
                ;

            contentType ??= "application/x-www-form-urlencoded";

            result.Add(contentType, new OpenApiMediaType
            {
                Schema = schema,
                Example = example
            });
        }

        return result;
    }

    private static string ToJsonString(this IEnumerable<PostmanUrlEncodedParameter> encodedParameters, Dictionary<string, string>? variables)
    {
        variables ??= new Dictionary<string, string>();
        var dictionary = encodedParameters.Where(x => x.Disabled != true)
            .ToDictionary(x => x.Key, y => y.Value?.ReplaceVariables(variables));
        return JsonConvert.SerializeObject(dictionary);
    }
}