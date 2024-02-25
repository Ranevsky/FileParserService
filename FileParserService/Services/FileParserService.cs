using FileParserService.Extensions;
using FileParserService.FileParsers.Interfaces;
using FileParserService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileParserService.Services;

public class FileParserService : IParserService
{
    private readonly IFileHandlerFabric _fileHandlerFabric;
    private readonly ILogger<FileParserService> _logger;
    private readonly IMessageService _messageService;

    public FileParserService(
        IFileHandlerFabric fileHandlerFabric,
        IMessageService messageService,
        ILogger<FileParserService> logger)
    {
        _fileHandlerFabric = fileHandlerFabric;
        _messageService = messageService;
        _logger = logger;
    }

    public async Task ParseAsync(FileStream fileStream, CancellationToken cancellationToken = default)
    {
        var (name, typeName) = fileStream.Name.GetFileType();

        var parser = _fileHandlerFabric.GetHandler(typeName);
        if (parser is null)
        {
            _logger.LogWarning("Format {FileFormat} is not support", typeName);

            return;
        }

        _logger.LogInformation("Parse file {FileName}", $"{name}.{typeName}");
        var message = await parser.HandleAsync(fileStream, cancellationToken);

        try
        {
            _logger.LogInformation("Message sent");
            await _messageService.SendSensorInformationAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(message: "Message not sent", exception: ex);

            throw;
        }
    }
}