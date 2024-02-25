using System.Xml.Linq;

namespace FileParserService.Services.Interfaces;

public interface IXmlToCsConverter
{
    Task CreateAsync(XElement xml, CancellationToken cancellationToken = default);
}