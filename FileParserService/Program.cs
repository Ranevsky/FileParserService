using FileParserService;
using FileParserService.Models;
using FileParserService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddServices(); })
    .ConfigureHostConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", false);
        config.AddEnvironmentVariables();
    })
    .Build();

using (var scopeService = host.Services.CreateScope())
{
    var serviceProvider = scopeService.ServiceProvider;
    var parserService = serviceProvider.GetRequiredService<IParserService>();
    var dataPath = serviceProvider.GetRequiredService<DataPath>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var tasks = new List<Task>();

    var filesFromDirectories = dataPath.Directories
        .Where(directoryPath =>
        {
            var isExists = Directory.Exists(directoryPath);
            if (!isExists)
            {
                logger.LogError("Directory {DirectoryPath} not found", directoryPath);
            }

            return isExists;
        })
        .SelectMany(Directory.GetFiles);
    var filePaths = dataPath.Files.Concat(filesFromDirectories);

    foreach (var filePath in filePaths)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object> { ["FilePath"] = filePath });

        var isFileExist = File.Exists(filePath);
        if (!isFileExist)
        {
            logger.LogError("File {FilePath} not found", filePath);

            continue;
        }

        var parseFileTask = Task.Run(async () =>
        {
            await using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await parserService.ParseAsync(fileStream);
        });

        tasks.Add(parseFileTask);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    var fileHandlingTasks = Task.WhenAll(tasks);

    try
    {
        await fileHandlingTasks;
    }
    catch
    {
        var aggregateException = fileHandlingTasks.Exception;
        if (aggregateException is null)
        {
            throw;
        }

        foreach (var ex in aggregateException.InnerExceptions)
        {
            logger.LogCritical(message: "Error not handled", exception: ex);
        }
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();