namespace FileParserService.Extensions;

public static class StringExtension
{
    public static (string Name, string Type) GetFileType(this string fileName)
    {
        var dotIndex = fileName.LastIndexOf('.');
        if (dotIndex < 0)
        {
            throw new FormatException("File format not found");
        }

        var slashIndex = Math.Max(fileName.LastIndexOf('\\'), fileName.LastIndexOf('/'));
        if (slashIndex < 0)
        {
            throw new FormatException("File name not found");
        }

        var dotLength = fileName.Length - dotIndex - 1;
        var type = fileName.Substring(dotIndex + 1, dotLength);
        var name = fileName.Substring(slashIndex + 1, fileName.Length - slashIndex - dotLength - 2);

        return (name, type);
    }
}