using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using XmlToCSharp.Extensions;
using XmlToCSharp.Models;

namespace XmlToCSharp;

public class XmlToCsFileGenerator
{
    private const string InitFileName = "InitData.json";

    private static readonly SemaphoreSlim SemaphoreSlim = new(1);
    private static ICollection<Class> _oldClasses = new List<Class>();
    private static bool _isInitialized;

    private readonly ILogger<XmlToCsFileGenerator> _logger;

    public XmlToCsFileGenerator(ILogger<XmlToCsFileGenerator> logger)
    {
        _logger = logger;
    }

    public async Task CreateAsync(
        string path,
        string @namespace,
        XElement element,
        CancellationToken cancellationToken = default)
    {
        var classes = element.ExtractClassInfo();
        await CreateFileAsync(path, @namespace, classes, cancellationToken);
    }

    private async Task CreateFileAsync(
        string path,
        string @namespace,
        IEnumerable<Class> classes,
        CancellationToken cancellationToken = default)
    {
        var classList = classes as ICollection<Class> ?? classes.ToList();
        await SemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await InitDataAsync(path, cancellationToken);

            CreateDirectoryIfNotExists(path);
            var hasChange = false;
            foreach (var @class in GetChangedClass(classList))
            {
                hasChange = true;
                var fileName = $"{@class.Name}.cs";
                var fileContent = GenerateFileContent(@class, @namespace);

                _logger.LogInformation("Create file {FileName}", fileName);
                var fullPath = Path.Combine(path, fileName);
                await File.WriteAllTextAsync(fullPath, fileContent, cancellationToken);
            }

            if (hasChange)
            {
                await SetDataAsync(path, cancellationToken);
            }
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    private async Task InitDataAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        var fullPath = Path.Combine(path, InitFileName);
        if (!File.Exists(fullPath))
        {
            _logger.LogTrace("Init file {FilePath} not found", fullPath);
            _oldClasses = new List<Class>();

            return;
        }

        _logger.LogInformation("Read init file");
        await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _oldClasses = await JsonSerializer.DeserializeAsync<ICollection<Class>>(
                          fs,
                          cancellationToken: cancellationToken)
                      ?? new List<Class>();
    }

    private async Task SetDataAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (_oldClasses.Count <= 0)
        {
            return;
        }

        _logger.LogInformation("Write init file");
        var fullPath = Path.Combine(path, InitFileName);
        await using var fs = new FileStream(fullPath, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, _oldClasses, cancellationToken: cancellationToken);
    }

    private static IEnumerable<Class> GetChangedClass(IEnumerable<Class> classes)
    {
        foreach (var newClass in classes)
        {
            var hasChange = false;
            var oldClass = _oldClasses.FirstOrDefault(oldClass => oldClass.Name == newClass.Name);
            if (oldClass is null)
            {
                _oldClasses.Add(newClass);

                yield return newClass;

                continue;
            }

            foreach (var oldField in oldClass.Fields)
            {
                var foundedField = newClass.Fields.FirstOrDefault(newField => newField.Name == oldField.Name);
                if (foundedField is null)
                {
                    if (oldField.Type.Has(FieldType.Nullable))
                    {
                        continue;
                    }

                    oldField.Type = oldField.Type.Add(FieldType.Nullable);
                    hasChange = true;
                }
                else
                {
                    if (!oldField.Type.Has(FieldType.Nullable) && foundedField.Type.Has(FieldType.Nullable))
                    {
                        oldField.Type = oldField.Type.Add(FieldType.Nullable);
                        hasChange = true;
                    }

                    if (foundedField.Type.Has(FieldType.NotPrimitive) ||
                        oldField.TypeName == oldField.Name ||
                        oldField.TypeName == foundedField.TypeName ||
                        oldField.TypeName == PrimitiveTypeConst.Object)
                    {
                        continue;
                    }

                    oldField.TypeName = oldField.TypeName == PrimitiveTypeConst.Int &&
                                        foundedField.TypeName == PrimitiveTypeConst.Double
                        ? PrimitiveTypeConst.Double
                        : PrimitiveTypeConst.Object;

                    hasChange = true;
                }
            }

            var newFields = newClass.Fields.Where(field => oldClass.Fields.All(x => x.Name != field.Name)).ToList();
            if (newFields.Count > 0)
            {
                foreach (var newField in newFields)
                {
                    oldClass.Fields.Add(newField);
                }

                hasChange = true;
            }

            if (hasChange)
            {
                yield return oldClass;
            }
        }
    }

    private static string GenerateFileContent(Class @class, string @namespace)
    {
        var properties = @class.Fields.Select(field =>
        {
            var type = string.Join(
                string.Empty,
                field.Type.Has(FieldType.Collection) ? $"ICollection<{field.TypeName}>" : field.TypeName,
                field.Type.Has(FieldType.Nullable) ? "?" : null);

            return $"public {type} {field.Name} {{ get; set; }}";
        });
        var formattedProperties = string.Join("\r\n    ", properties);

        var classFileText =
            $$"""
              namespace {{@namespace}};

              public class {{@class.Name}}
              {
                  {{formattedProperties}}
              }
              """;

        return classFileText;
    }

    private void CreateDirectoryIfNotExists(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        _logger.LogTrace("Create directory {FileName}", path);
        Directory.CreateDirectory(path);
    }
}