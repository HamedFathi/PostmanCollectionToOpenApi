// ReSharper disable UnusedMember.Global

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanRequestUnionExtensions
{
    public static string? GetPath(this PostmanRequestUnion? requestUnion)
    {
        if (requestUnion == null) return null;

        var data = requestUnion.Value.RequestClass.Url?.UrlClass.Path?.AnythingArray
                .Select(x => x.String)
                .Aggregate((a, b) => $"{a}/{b}")
            ;
        return "/" + data;
    }

    public static string? GetRawUrl(this PostmanRequestUnion? requestUnion)
    {
        return requestUnion?.RequestClass.Url?.UrlClass.Raw;
    }

    public static string? GetRawUrl(this PostmanRequestUnion requestUnion)
    {
        return requestUnion.RequestClass.Url?.UrlClass.Raw;
    }
}