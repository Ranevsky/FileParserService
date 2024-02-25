using FileParserService.FileParsers;
using FileParserService.FileParsers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FileParserService.Services;

public class FileHandlerFabric : IFileHandlerFabric
{
    private readonly IServiceProvider _serviceProvider;

    public FileHandlerFabric(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileHandler? GetHandler(string fileType)
    {
        var parser = fileType switch
        {
            "xml" => _serviceProvider.GetRequiredService<XmlHandler>(),
            _ => null,
        };

        return parser;
    }
}