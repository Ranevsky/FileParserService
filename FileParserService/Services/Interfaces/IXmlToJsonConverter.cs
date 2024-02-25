using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace FileParserService.Services.Interfaces;

public interface IXmlToJsonConverter
{
    JsonObject Convert(XElement element, CancellationToken cancellationToken = default);
}