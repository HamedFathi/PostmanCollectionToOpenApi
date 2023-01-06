// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System.ComponentModel;

namespace PostmanCollectionToOpenApi.Enums;

public enum OpenApiValueType
{
    Boolean,
    Integer,
    Int32,
    Int64,
    Number,
    Float,
    Double,
    String,
    Date,

    [Description("date-time")]
    DateTime,

    Password,
    Byte,
    Binary,
    Email,
    Uuid,
    Uri,
    HostName,
    IPv4,
    IPv6,
    Null,
    Array,
    Object
}