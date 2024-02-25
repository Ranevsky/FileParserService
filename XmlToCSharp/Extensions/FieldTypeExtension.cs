using XmlToCSharp.Models;

namespace XmlToCSharp.Extensions;

internal static class FieldTypeExtension
{
    public static FieldType Add(this FieldType field, FieldType value)
    {
        return field | value;
    }

    public static bool Has(this FieldType field, FieldType value)
    {
        return (field & value) == value;
    }
}