using System.Text.Json.Nodes;
using System.Xml.Linq;
using XmlToJson.Extensions;

namespace XmlToJson;

public static class XmlToJsonConverter
{
    public static JsonObject Convert(XElement element)
    {
        return ParseElement(element);
    }

    private static JsonObject ParseElement(XElement element)
    {
        JsonObject jsonObject;
        if (!element.HasElements)
        {
            jsonObject = new JsonObject { { element.GetName(), element.GetTypedValue() } };

            if (element.HasAttributes)
            {
                jsonObject.AddAttributes(element);
            }
        }
        else
        {
            var groupedElements = element.GetGroupedElements();
            jsonObject = ParseGroupedElements(groupedElements);
            if (element.HasAttributes)
            {
                jsonObject.AddAttributes(element);
            }
        }

        return jsonObject;
    }

    private static JsonObject ParseGroupedElements(IEnumerable<ICollection<XElement>> groupElements)
    {
        var jsonResult = new JsonObject();
        foreach (var groupElement in groupElements)
        {
            if (groupElement.Count > 1)
            {
                var (name, array) = ParseArray(groupElement);
                jsonResult.Add(name, array);
            }
            else
            {
                var firstElement = groupElement.First();
                var (name, value) = ParseObject(firstElement);
                jsonResult.Add(name, value);
            }
        }

        return jsonResult;
    }


    private static (string Name, JsonArray Array) ParseArray(ICollection<XElement> elements)
    {
        var jsonArray = new JsonArray();
        var name = elements.First().GetName();

        foreach (var element in elements)
        {
            if (!element.HasElements)
            {
                GetJsonObjectAttribute(element, out var value);
                jsonArray.Add(value);
            }
            else
            {
                var parsedElement = ParseGroupedElements(element.GetGroupedElements());

                if (element.HasAttributes)
                {
                    parsedElement.AddAttributes(element);
                }

                jsonArray.Add(parsedElement);
            }
        }

        return (name, jsonArray);
    }

    private static (string Name, JsonNode Value) ParseObject(XElement element)
    {
        var name = element.GetName();

        JsonNode value;
        if (element.HasElements)
        {
            value = ParseElement(element);
        }
        else
        {
            GetJsonObjectAttribute(element, out value);
        }

        return (name, value);
    }

    private static void GetJsonObjectAttribute(XElement element, out JsonNode node)
    {
        node = element.GetTypedValue();
        if (!element.HasAttributes)
        {
            return;
        }

        var jsonObject = new JsonObject { { "#value", node } };
        jsonObject.AddAttributes(element);

        node = jsonObject;
    }
}