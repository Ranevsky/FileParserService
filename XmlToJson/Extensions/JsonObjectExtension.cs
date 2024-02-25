using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace XmlToJson.Extensions;

internal static class JsonObjectExtension
{
    public static void AddAttributes(this JsonObject jsonObject, XElement element)
    {
        foreach (var attribute in element.GetAttributes())
        {
            jsonObject.Add(attribute.Name, attribute.Value);
        }
    }
}