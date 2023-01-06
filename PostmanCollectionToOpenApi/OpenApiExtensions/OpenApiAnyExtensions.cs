namespace PostmanCollectionToOpenApi.OpenApiExtensions;

internal static class OpenApiAnyExtensions
{
    public static IOpenApiAny ToExample(this string value, Dictionary<string, string>? variables)
    {
        variables ??= new Dictionary<string, string>();
        value = value.ReplaceVariables(variables);
        var openApiValueType = value.ToOpenApiValueType();

        if (value == string.Empty)
            return new OpenApiString(value);

        switch (openApiValueType)
        {
            case OpenApiValueType.Boolean:
                return new OpenApiBoolean(Convert.ToBoolean(value));

            case OpenApiValueType.Integer:
            case OpenApiValueType.Int32:
                return new OpenApiInteger(Convert.ToInt32(value));

            case OpenApiValueType.Int64:
                return new OpenApiLong(Convert.ToInt64(value));

            case OpenApiValueType.Number:
            case OpenApiValueType.Float:
                return new OpenApiFloat(Convert.ToSingle(value));

            case OpenApiValueType.Double:
                return new OpenApiDouble(Convert.ToDouble(value));

            case OpenApiValueType.String:
            case OpenApiValueType.Password:
            case OpenApiValueType.Email:
            case OpenApiValueType.Uuid:
            case OpenApiValueType.Uri:
            case OpenApiValueType.HostName:
            case OpenApiValueType.IPv4:
            case OpenApiValueType.IPv6:
            case OpenApiValueType.Byte: // ?
            case OpenApiValueType.Binary: // ?
                return new OpenApiString(value);

            case OpenApiValueType.Date:
                return new OpenApiDate(Convert.ToDateTime(value));

            case OpenApiValueType.DateTime:
                return new OpenApiDateTime(Convert.ToDateTime(value));

            case OpenApiValueType.Null:
                return new OpenApiNull();

            case OpenApiValueType.Array:
                return value.ToOpenApiArray(variables);

            case OpenApiValueType.Object:
                return JToken.Parse(value).ToOpenApiObject(variables);

            default:
                return new OpenApiString(value);
        }
    }
}