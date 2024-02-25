using XmlToCSharp.Models;

namespace XmlToCSharp.Extensions;

internal static class FieldsExtensions
{
    public static bool Matches(this IEnumerable<Field> input, IEnumerable<Field> other)
    {
        return input.OrderBy(f => f.Name).SequenceEqual(other.OrderBy(f => f.Name));
    }
}