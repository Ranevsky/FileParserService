namespace FileParserService.FileParsers.Interfaces;

public interface IFileHandlerFabric
{
    IFileHandler? GetHandler(string fileType);
}