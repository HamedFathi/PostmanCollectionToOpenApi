namespace PostmanCollectionToOpenApi.OpenApiExtensions;

internal static class OpenApiObjectExtensions
{
    public static OpenApiObject ToOpenApiObject(this JToken jToken, Dictionary<string, string>? variables)
    {
        variables ??= new Dictionary<string, string>();
        var openApiObject = new OpenApiObject();
        foreach (var jsonDetail in jToken.GetJTokenDetails())
        {
            openApiObject = openApiObject.OpenApiObjectMaker(jsonDetail.Path, jsonDetail.JTokenString.ToExample(variables));
        }

        return openApiObject;
    }

    private static OpenApiObject OpenApiObjectMaker(this OpenApiObject openApiObject, string[] path, IOpenApiAny? childValue)
    {
        IOpenApiAny prevOpenApiAny = openApiObject;
        for (var i = 0; i < path.Length; i++)
        {
            var pathDetail = path[i].GetPathDetail();
            var name = pathDetail.name;
            var index = pathDetail.index;
            var isChild = i + 1 == path.Length; // Is child
            var isArray = index != -1; // path related to an array or object

            // Child, the place to add value
            if (isChild)
            {
                // Child in an Array
                if (isArray)
                {
                    // Array is there we should just add value.
                    if (prevOpenApiAny.CastTo<OpenApiObject>().Keys.Contains(name))
                    {
                        prevOpenApiAny.CastTo<OpenApiObject>()[name].CastTo<OpenApiArray>().Add(childValue);
                    }
                    // Array does not exist and we should create it first.
                    else
                    {
                        var arr = new OpenApiArray { childValue };
                        prevOpenApiAny.CastTo<OpenApiObject>().Add(name, arr);
                    }
                }
                // Child in an Object
                else
                {
                    prevOpenApiAny.CastTo<OpenApiObject>().Add(name, childValue);
                }
            }
            // Parent
            else
            {
                // Parent is an array
                if (isArray)
                {
                    // Parent does not exist
                    if (!prevOpenApiAny.CastTo<OpenApiObject>().Keys.Contains(name))
                    {
                        var oao = new OpenApiObject();
                        var oaa = new OpenApiArray { oao };
                        prevOpenApiAny.CastTo<OpenApiObject>().Add(name, oaa);
                        prevOpenApiAny = oao;
                    }
                    // Parent exists
                    else
                    {
                        var array = (OpenApiArray)prevOpenApiAny.CastTo<OpenApiObject>()[name];
                        var hasValidIndex = array.Count >= index + 1;
                        // Parent & Index both are available.
                        if (hasValidIndex)
                        {
                            prevOpenApiAny = array[index];
                        }
                        // Parent is available but index is one behind and we should add it.
                        else
                        {
                            var oao = new OpenApiObject();
                            array.Add(oao);
                            prevOpenApiAny = oao;
                        }
                    }
                }
                // Parent is an object
                else
                {
                    // Object does not exist.
                    if (!prevOpenApiAny.CastTo<OpenApiObject>().Keys.Contains(name))
                    {
                        var oao = new OpenApiObject();
                        prevOpenApiAny.CastTo<OpenApiObject>().Add(name, oao);
                        prevOpenApiAny = oao;
                    }
                    // Object exists.
                    else
                    {
                        prevOpenApiAny = prevOpenApiAny.CastTo<OpenApiObject>()[name];
                    }
                }
            }
        }

        return openApiObject;
    }
}