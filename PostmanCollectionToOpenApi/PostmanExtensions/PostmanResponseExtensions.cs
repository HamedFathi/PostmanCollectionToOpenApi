// ReSharper disable UnusedMember.Global

using PostmanCollectionToOpenApi.OpenApiExtensions;

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanResponseExtensions
{
    public static string? GetRawUrl(this PostmanResponse? response)
    {
        return response?.ResponseClass.OriginalRequest?.RequestClass.Url?.UrlClass.Raw;
    }

    public static string? GetRawUrl(this PostmanResponse response)
    {
        return response.ResponseClass.OriginalRequest?.RequestClass.Url?.UrlClass.Raw;
    }

    public static IList<string?> GetRawUrl(this IList<PostmanResponse> responses)
    {
        return responses.Select(x => x.GetRawUrl()).ToList();
    }

    public static OpenApiResponses ToOpenApiResponses(this IList<PostmanResponse> responses, Dictionary<string, string>? variables)
    {
        var openApiResponses = new OpenApiResponses();
        foreach (var response in responses)
        {
            // Description of response (Name)?
            var code = response.ResponseClass.Code.ToString();
            var headers = response.ResponseClass.Header?.AnythingArray
                  .Where(x =>
                      string.Equals(x.Header.Key, "content-type", StringComparison.OrdinalIgnoreCase) &&
                      x.Header.Disabled != true).ToList();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    var openApiResponse = new OpenApiResponse();
                    var openApiMedia = new OpenApiMediaType();
                    var body = response.ResponseClass.Body;
                    if (!string.IsNullOrEmpty(body))
                    {
                        openApiMedia.Schema = body.ToOpenApiSchema(variables);
                        openApiMedia.Example = body.ToExample(variables);
                    }
                    openApiResponse.Content.Add(header.Header.Value, openApiMedia);
                    openApiResponses.Add(code, openApiResponse);
                }
            }
        }
        return openApiResponses;
    }
}