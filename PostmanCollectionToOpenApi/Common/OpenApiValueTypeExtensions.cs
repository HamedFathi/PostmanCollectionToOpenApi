namespace PostmanCollectionToOpenApi.Common;

internal static class OpenApiValueTypeExtensions
{
    internal static (string? type, string? format) GetTypeAndFormat(this OpenApiValueType openApiValueType)
    {
        return openApiValueType switch
        {
            OpenApiValueType.Boolean => ("bool", null),
            OpenApiValueType.Integer => ("integer", null),
            OpenApiValueType.Int32 => ("integer", "int32"),
            OpenApiValueType.Int64 => ("integer", "int64"),
            OpenApiValueType.Number => ("number", null),
            OpenApiValueType.Float => ("number", "float"),
            OpenApiValueType.Double => ("number", "double"),
            OpenApiValueType.String => ("string", null),
            OpenApiValueType.Date => ("string", "date"),
            OpenApiValueType.DateTime => ("string", "date-time"),
            OpenApiValueType.Password => ("string", "password"),
            OpenApiValueType.Byte => ("string", "byte"),
            OpenApiValueType.Binary => ("string", "binary"),
            OpenApiValueType.Email => ("string", "email"),
            OpenApiValueType.Uuid => ("string", "uuid"),
            OpenApiValueType.Uri => ("string", "uri"),
            OpenApiValueType.HostName => ("string", "hostname"),
            OpenApiValueType.IPv4 => ("string", "ipv4"),
            OpenApiValueType.IPv6 => ("string", "ipv6"),
            OpenApiValueType.Null => (null, null),
            OpenApiValueType.Array => ("array", null),
            OpenApiValueType.Object => ("object", null),
            _ => throw new ArgumentOutOfRangeException(nameof(openApiValueType), openApiValueType, null)
        };
    }

    internal static OpenApiValueType ToOpenApiValueType(this string value)
    {
        if (value == string.Empty)
            return OpenApiValueType.String;

        if (value == "null")
            return OpenApiValueType.Null;

        if (value is "true" or "false")
            return OpenApiValueType.Boolean;

        if (int.TryParse(value, out _))
        {
            return OpenApiValueType.Int32;
        }

        if (long.TryParse(value, out _))
        {
            return OpenApiValueType.Int64;
        }

        if (float.TryParse(value, out _))
        {
            return OpenApiValueType.Float;
        }

        if (double.TryParse(value, out _))
        {
            return OpenApiValueType.Double;
        }

        if (DateTime.TryParse(value, out _) && value.Contains("T"))
        {
            return OpenApiValueType.DateTime;
        }

        if (DateTime.TryParse(value, out _))
        {
            return OpenApiValueType.Date;
        }

        if (Guid.TryParse(value, out _))
        {
            return OpenApiValueType.Uuid;
        }

        if (value.IsJsonText())
        {
            return OpenApiValueType.Object;
        }

        if (value.IsArrayText())
        {
            return OpenApiValueType.Array;
        }

        if (value.IsIpAddressV4())
        {
            return OpenApiValueType.IPv4;
        }

        if (value.IsIpAddressV6())
        {
            return OpenApiValueType.IPv6;
        }

        if (value.IsValidUri())
        {
            return OpenApiValueType.Uri;
        }

        if (value.IsValidEmail())
        {
            return OpenApiValueType.Email;
        }

        return OpenApiValueType.String;
    }
}