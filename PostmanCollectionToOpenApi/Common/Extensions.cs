using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace PostmanCollectionToOpenApi.Common;

internal static class Extensions
{
    private static readonly Regex EmailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.Compiled);
    private static readonly Regex UriRegex = new Regex(@"^\w+:(\/?\/?)[^\s]+$", RegexOptions.Compiled);

    internal static T CastTo<T>(this object o) => (T)o;

    internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        return source.Where(element => seenKeys.Add(keySelector(element)));
    }

    internal static IEnumerable<T> FlattenByStack<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>?>? getChildren)
    {
        var stack = new Stack<T>();
        foreach (var item in items)
            stack.Push(item);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            var children = getChildren?.Invoke(current);
            if (children != null)
                foreach (var child in children)
                    stack.Push(child);
        }
    }

    internal static string? GetDescription(this Enum @enum, bool returnEnumNameInsteadOfNull = false)
    {
        if (@enum == null) throw new ArgumentNullException(nameof(@enum));

        return
            @enum
                .GetType()
                .GetMember(@enum.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description
            ?? (!returnEnumNameInsteadOfNull ? null : @enum.ToString());
    }

    internal static string GetHostWithScheme(this string uriText)
    {
        var uri = new Uri(uriText);
        return uri.Scheme + Uri.SchemeDelimiter + uri.Host;
    }

    internal static bool IsArrayText(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var txt = text.Trim().RemoveAllWhiteSpaces().Trim().Trim(',').Trim(';');
        var result = txt.StartsWith("[") && txt.EndsWith("]");
        return result;
    }

    internal static bool IsIpAddressV4(this string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var address)) return false;

        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => true,
            _ => false
        };
    }

    internal static bool IsIpAddressV6(this string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var address)) return false;

        return address.AddressFamily switch
        {
            AddressFamily.InterNetworkV6 => true,
            _ => false
        };
    }

    internal static bool IsJsonText(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var txt = text.Trim().RemoveAllWhiteSpaces().Trim().Trim(',').Trim(';');
        var result = txt.StartsWith("{\"") && txt.EndsWith("}") && txt.Contains("\":\"");
        return result;
    }

    internal static bool IsValidEmail(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;
        return EmailRegex.IsMatch(str);
    }

    internal static bool IsValidUri(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;
        return UriRegex.IsMatch(str);
    }

    internal static string RemoveAllWhiteSpaces(this string text)
    {
        return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, " ", string.Empty)
            .Replace("\n", string.Empty).Replace("\r", string.Empty)
            .Trim();
    }

    internal static string RemoveArrayIndex(this string str)
    {
        var status = str.Contains("[") && str.EndsWith("]");
        if (!status)
            return str;

        var startIndex = str.LastIndexOf('[');
        var indexer = str.Substring(startIndex);
        str = str.Replace(indexer, string.Empty);
        return str;
    }

    internal static string Replace(this string text, int startIndex, int count, string replacement)
    {
        return text.Remove(startIndex, count).Insert(startIndex, replacement);
    }

    internal static string ReplaceRegex(this string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement);
    }
}