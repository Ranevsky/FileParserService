using System.Text.Json.Nodes;

namespace XmlToJson.Extensions;

internal static class StringExtension
{
    public static JsonNode GetTypedValue(this string value)
    {
        object result;
        if (bool.TryParse(value, out var boolResult))
        {
            result = boolResult;
        }
        else if (double.TryParse(value, out var numberResult))
        {
            result = numberResult;
        }
        else
        {
            result = value;
        }

        return JsonValue.Create(result)!;
    }
}