namespace FileParserService.Services.Interfaces;

public interface IMessageService
{
    Task SendSensorInformationAsync(string json, CancellationToken cancellationToken = default);
}