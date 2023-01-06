// ReSharper disable UnusedMember.Global

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
}