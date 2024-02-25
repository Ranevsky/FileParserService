using FileParserService.Extensions;
using FileParserService.FileParsers;
using FileParserService.FileParsers.Interfaces;
using FileParserService.Models;
using FileParserService.Services;
using FileParserService.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FileParserService;

public static class ServiceConfiguration
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IParserService, Services.FileParserService>();
        services.AddScoped<IFileHandlerFabric, FileHandlerFabric>();
        services.AddScoped<IMessageService, RabbitMessageService>();
        services.AddScoped<IFileHandlerFabric, FileHandlerFabric>();
        services.AddScoped<IXmlToCsConverter, XmlToCsConverter>();
        services.AddScoped<IXmlToJsonConverter, XmlToJsonConverter>();
        services.AddScoped<XmlHandler>();
        services.AddScoped<RabbitMqConnectionConfiguration>();

        services.AddFromConfiguration<DataPath>("DataPath", ServiceLifetime.Scoped);
        services.AddFromConfiguration<RabbitMqConnectionConfiguration>("Rabbit", ServiceLifetime.Scoped);
        services.AddFromConfiguration<GeneratedFileSetting>("GeneratedFileSetting", ServiceLifetime.Scoped);
    }
}