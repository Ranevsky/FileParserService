using System.Text;
using System.Text.Json.Nodes;
using System.Web;
using System.Xml.Linq;
using FileParserService.Extensions;
using FileParserService.FileParsers.Interfaces;
using FileParserService.Models;
using FileParserService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileParserService.FileParsers;

public class XmlHandler : IFileHandler
{
    private readonly ILogger<XmlHandler> _logger;
    private readonly IXmlToCsConverter _xmlToCsConverter;
    private readonly IXmlToJsonConverter _xmlToJsonConverter;

    public XmlHandler(
        IXmlToCsConverter xmlToCsConverter,
        IXmlToJsonConverter xmlToJsonConverter,
        ILogger<XmlHandler> logger)
    {
        _xmlToCsConverter = xmlToCsConverter;
        _xmlToJsonConverter = xmlToJsonConverter;
        _logger = logger;
    }

    public async Task<string> HandleAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Load xml");
        var xml = await GetXmlAsync(fileStream);
        var element = XElement.Parse(xml);

        var convertToJsonTask = Task.Run(() =>
            {
                _logger.LogInformation("Convert xml to json");
                var json = _xmlToJsonConverter.Convert(element);

                _logger.LogInformation("Randomize module state");
                RandomizeModuleState(json);

                return json.ToString();
            },
            cancellationToken);

        var createCsTask = Task.Run(() =>
        {
            _logger.LogInformation("Convert xml to .cs file");

            return _xmlToCsConverter.CreateAsync(element, cancellationToken);
        }, cancellationToken);

        await Task.WhenAll(convertToJsonTask, createCsTask);

        return await convertToJsonTask;
    }

    private static async Task<string> GetXmlAsync(Stream fileStream)
    {
        var xml = new StringBuilder();
        using var streamReader = new StreamReader(fileStream);
        const int bufferSize = 4096;
        var buffer = new char[bufferSize];
        int bytesRead;

        while ((bytesRead = await streamReader.ReadBlockAsync(buffer, 0, bufferSize)) > 0)
        {
            xml.Append(buffer, 0, bytesRead);
        }

        var text = xml.ToString();
        text = HttpUtility.HtmlDecode(text);
        text = text.AsSpan().RemoveTag("<?xml", "?>").ToString();

        return text;
    }

    private static void RandomizeModuleState(JsonNode jsonObject)
    {
        var deviceStatuses = jsonObject["DeviceStatus"]?.AsArray();
        if (deviceStatuses is null)
        {
            return;
        }

        foreach (var deviceStatus in deviceStatuses)
        {
            if (deviceStatus is null)
            {
                continue;
            }

            var rapidControlStatus = deviceStatus["RapidControlStatus"]?.AsObject();
            if (rapidControlStatus is null)
            {
                continue;
            }

            var controlStatuses = rapidControlStatus
                .Select(controlStatus => controlStatus.Value?.AsObject())
                .Where(controlStatus => controlStatus is not null);

            foreach (var controlStatus in controlStatuses)
            {
                controlStatus!["ModuleState"] = ModuleState.States.GetRandom();
            }
        }
    }
}