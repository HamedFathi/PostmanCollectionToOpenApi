namespace PostmanCollectionToOpenApi.OpenApiExtensions;

public static class OpenApiArrayExtensions
{
    private static readonly Regex ArrayIndexRegex = new(@"\[(.*)\]", RegexOptions.Singleline);

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

        var openApiArray = new OpenApiArray();

        var match = ArrayIndexRegex.Match(value);
        var group = match.Groups[1].Value;
        var items = group.Split(',').Select(x => x.Trim()).ToList();

        var isString = items.All(x => x.StartsWith("\""));

        var vType = items[0].ToOpenApiValueType();
        if (isString)
        {
            openApiArray.AddRange(items.Select(x => new OpenApiString(x.Trim('"'))));
        }
        else
        {
            switch (vType)
            {
                case OpenApiValueType.Boolean:
                    openApiArray.AddRange(items.Select(x => new OpenApiBoolean(Convert.ToBoolean(x.ToLower()))));
                    break;

                case OpenApiValueType.Integer:
                case OpenApiValueType.Int32:
                    openApiArray.AddRange(items.Select(x => new OpenApiInteger(Convert.ToInt32(x))));
                    break;

                case OpenApiValueType.Int64:
                    openApiArray.AddRange(items.Select(x => new OpenApiLong(Convert.ToInt64(x))));
                    break;

                case OpenApiValueType.Number:
                case OpenApiValueType.Float:
                    openApiArray.AddRange(items.Select(x => new OpenApiFloat(Convert.ToSingle(x))));
                    break;

                case OpenApiValueType.Double:
                    openApiArray.AddRange(items.Select(x => new OpenApiDouble(Convert.ToDouble(x))));
                    break;

                case OpenApiValueType.Date:
                    openApiArray.AddRange(items.Select(x => new OpenApiDate(Convert.ToDateTime(x))));
                    break;

                case OpenApiValueType.DateTime:
                    openApiArray.AddRange(items.Select(x => new OpenApiDateTime(Convert.ToDateTime(x))));
                    break;

                default:
                    openApiArray.AddRange(items.Select(x => new OpenApiString(x.Trim('"'))));
                    break;
            }
        }

        return openApiArray;
    }
}