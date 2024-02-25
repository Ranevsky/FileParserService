using System.Xml.Linq;
using FileParserService.Models;
using FileParserService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using XmlToCSharp;

namespace FileParserService.Services;

public class XmlToCsConverter : IXmlToCsConverter
{
    private readonly GeneratedFileSetting _generatedFileSetting;
    private readonly ILoggerFactory _loggerFactory;

    public XmlToCsConverter(
        GeneratedFileSetting generatedFileSetting,
        ILoggerFactory loggerFactory)
    {
        _generatedFileSetting = generatedFileSetting;
        _loggerFactory = loggerFactory;
    }

    public async Task CreateAsync(XElement element, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<XmlToCsFileGenerator>();
        var fileGenerator = new XmlToCsFileGenerator(logger);

        var filePath = _generatedFileSetting.Path;
        var @namespace = _generatedFileSetting.Namespace;

        await fileGenerator.CreateAsync(filePath, @namespace, element, cancellationToken);
    }
}