namespace FileParserService.Services.Interfaces;

public interface IParserService
{
    Task ParseAsync(FileStream fileStream, CancellationToken cancellationToken = default);
}