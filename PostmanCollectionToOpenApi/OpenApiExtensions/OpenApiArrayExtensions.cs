namespace PostmanCollectionToOpenApi.OpenApiExtensions;

public static class OpenApiArrayExtensions
{
    public static OpenApiArray ToOpenApiArray(this string value, Dictionary<string, string>? variables = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        variables ??= new Dictionary<string, string>();
        value = value.ReplaceVariables(variables);
        var openApiValueType = value.ToOpenApiValueType();
        if (openApiValueType != OpenApiValueType.Array)
        {
            throw new Exception($"{nameof(value)} is not an array.");
        }

        value = value.ConvertJavascriptObjectOrArrayToJson(Constants.Separator);
        var openApiObject = JToken.Parse(value).ToOpenApiObject(variables);

        return openApiObject[Constants.Separator].CastTo<OpenApiArray>();
    }
}