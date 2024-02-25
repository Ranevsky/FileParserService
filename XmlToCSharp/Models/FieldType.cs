namespace XmlToCSharp.Models;

[Flags]
internal enum FieldType : byte
{
    NotPrimitive = 1 << 0,
    Nullable = 1 << 1,
    Collection = 1 << 2,
}