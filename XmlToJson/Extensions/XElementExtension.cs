using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace XmlToJson.Extensions;

internal static class XElementExtension
{
    public static string GetName(this XElement element)
    {
        return element.Name.GetName();
    }

    public static JsonNode GetTypedValue(this XElement element)
    {
        var value = element.Value;
        object result;
        if (bool.TryParse(value, out var boolResult))
        {
            result = boolResult;
        }
        else if (decimal.TryParse(value, out var numberResult))
        {
            result = numberResult;
        }
        else
        {
            result = value;
        }

        return JsonValue.Create(result)!;
    }

    public static Type GetValueType(this JsonValue jsonValue)
    {
        var value = jsonValue.GetValue<object>();
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.False => typeof(bool),
                JsonValueKind.True => typeof(bool),
                JsonValueKind.Number => typeof(double),
                JsonValueKind.String => typeof(string),
                var _ => typeof(JsonElement),
            };
        }

        return value.GetType();
    }

    public static IEnumerable<ICollection<XElement>> GetGroupedElements(this XElement element)
    {
        return element
            .Elements()
            .GroupBy(
                xElement => xElement.GetName(),
                (_, enumerable) =>
                {
                    var elementsList = enumerable as ICollection<XElement> ?? enumerable.ToList();

                    return elementsList;
                });
    }

    public static IEnumerable<(string Name, JsonNode Value)> GetAttributes(this XElement element)
    {
        var attributes = element
            .Attributes()
            .Select(x => (Name: $"@{x.Name.GetName()}", Value: x.Value.GetTypedValue()));

        foreach (var attribute in attributes)
        {
            yield return (attribute.Name, attribute.Value);
        }
    }
}