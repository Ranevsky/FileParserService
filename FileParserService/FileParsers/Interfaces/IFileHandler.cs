namespace FileParserService.FileParsers.Interfaces;

public interface IFileHandler
{
    Task<string> HandleAsync(Stream fileStream, CancellationToken cancellationToken = default);
}