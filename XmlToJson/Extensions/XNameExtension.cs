using System.Xml.Linq;

namespace XmlToJson.Extensions;

internal static class XNameExtension
{
    public static string GetName(this XName name)
    {
        return name.LocalName;
    }
}