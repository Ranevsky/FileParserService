using System.Text;
using FileParserService.Models;
using FileParserService.Services.Interfaces;
using RabbitMQ.Client;

namespace FileParserService.Services;

public class RabbitMessageService : IMessageService, IDisposable
{
    private const string SensorEventName = "SensorEvent";

    private IModel? _channel;
    private IConnection? _connection;

    private bool _isDisposed;

    public RabbitMessageService(RabbitMqConnectionConfiguration configuration)
    {
        var factory = configuration.CreateConnectionFactory();
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task SendSensorInformationAsync(string json, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(RabbitMessageService));

        DeclareRoute(_channel!);

        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            SensorEventName,
            string.Empty,
            null,
            body);

        return Task.CompletedTask;
    }

    private static void DeclareRoute(IModel channel)
    {
        channel.ExchangeDeclare(
            SensorEventName,
            ExchangeType.Fanout,
            true);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            DisposeInstance(ref _channel);
            DisposeInstance(ref _connection);
        }

        _isDisposed = true;

        return;

        static void DisposeInstance<TDisposable>(ref TDisposable? disposable)
            where TDisposable : class, IDisposable
        {
            if (disposable is null)
            {
                return;
            }

            disposable.Dispose();
            disposable = null;
        }
    }
}