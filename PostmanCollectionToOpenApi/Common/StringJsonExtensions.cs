namespace PostmanCollectionToOpenApi.Common;

internal static class StringJsonExtensions
{
    internal static readonly Regex JsonKeyRegex = new("(.+):", RegexOptions.Singleline);

    internal static string ConvertJavascriptObjectOrArrayToJson(this string jsObjectOrArrayString, string rootNameOfArray = "Root", bool formatted = false)
    {
        var isValid = jsObjectOrArrayString.IsJsonText();
        if (isValid)
        {
            return jsObjectOrArrayString;
        }
        var value = jsObjectOrArrayString.Trim().Trim(';').Trim(',').Trim('=');
        var text = value.RemoveAllWhiteSpaces().Trim();
        var isArray = text.StartsWith("[") && text.EndsWith("]");
        var isObject = text.StartsWith("{") && text.EndsWith("}");

        var matches = JsonKeyRegex.Matches(jsObjectOrArrayString);
        if (matches.Count == 0)
        {
            if (isArray)
            {
                var result = $"{{ \"{rootNameOfArray}\" : {jsObjectOrArrayString} }}";
                return formatted ? result.ToFormattedJson() : result;
            }
            if (isObject)
            {
                return formatted ? jsObjectOrArrayString.ToFormattedJson() : jsObjectOrArrayString;
            }
            return jsObjectOrArrayString;
        }
        do
        {
            jsObjectOrArrayString =
                jsObjectOrArrayString.Replace(matches[0].Index, matches[0].Length, $"\"{matches[0].Groups[1].Value}\":");
            matches = JsonKeyRegex.Matches(jsObjectOrArrayString);
        } while (matches.Count > 0);

        if (isArray)
        {
            return $"{{ \"{rootNameOfArray}\" : {jsObjectOrArrayString} }}";
        }
        if (isObject)
        {
            return jsObjectOrArrayString;
        }
        return jsObjectOrArrayString;
    }

    internal static string[] SplitPath(this string path)
    {
        if (path.Contains("['") || path.Contains("[\""))
        {
            path = path.Replace("['", Constants.Separator);
            path = path.Replace("']", string.Empty);
            path = path.Replace("[\"", Constants.Separator);
            path = path.Replace("\"]", string.Empty);
            return path.Split(new[] { Constants.Separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        return path.Split('.');
    }

    internal static string ToFormattedJson(this string jsonText)
    {
        jsonText = jsonText.Trim().Trim(',').Trim(';');
        return JToken.Parse(jsonText).ToString(Formatting.Indented);
    }
}