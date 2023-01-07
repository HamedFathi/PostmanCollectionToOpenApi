namespace PostmanCollectionToOpenApi.PostmanExtensions.PostmanVariables;

internal static class PostmanVariableExtensions
{
    internal static Dictionary<string, string> MergeVariables(this IEnumerable<PostmanVariable>? variables, IEnumerable<PostmanEnvironmentVariable>? values)
    {
        var result = new Dictionary<string, string>();

        if (values != null)
        {
            foreach (var vv in values)
            {
                if (!string.IsNullOrWhiteSpace(vv.Key) && vv.Enabled == true)
                {
                    result.Add(vv.Key.GetPostmanVariable(), vv.Value);
                }
            }
        }

        if (variables != null)
        {
            foreach (var variable in variables)
            {
                if (variable is { Value: { } } && !string.IsNullOrWhiteSpace(variable.Key) && variable.Disabled != true)
                {
                    var key = variable.Key.GetPostmanVariable();
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, variable.Value?.ToString()!);
                    }
                }
            }
        }
        return result;
    }
}