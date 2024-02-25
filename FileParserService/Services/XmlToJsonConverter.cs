using System.Text.Json.Nodes;
using System.Xml.Linq;
using FileParserService.Services.Interfaces;

namespace FileParserService.Services;

public class XmlToJsonConverter : IXmlToJsonConverter
{
    public JsonObject Convert(XElement element, CancellationToken cancellationToken = default)
    {
        var json = XmlToJson.XmlToJsonConverter.Convert(element);

        return json;
    }
}