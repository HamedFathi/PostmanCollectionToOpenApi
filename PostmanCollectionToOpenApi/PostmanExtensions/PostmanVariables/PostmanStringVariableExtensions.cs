namespace PostmanCollectionToOpenApi.PostmanExtensions.PostmanVariables;

internal static class PostmanStringVariableExtensions
{
    internal static string GetPostmanVariable(this string variable)
    {
        if (string.IsNullOrWhiteSpace(variable))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(variable));

        return $"{{{{{variable.Trim()}}}}}";
    }

    internal static string ReplaceVariables(this string text, Dictionary<string, string>? variables)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        if (variables == null || variables.Count == 0) return text;

        foreach (var variable in variables)
        {
            text = text.Replace(variable.Key, variable.Value);
        }

        return text;
    }
}