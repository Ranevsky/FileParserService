namespace FileParserService.Models;

public class DataPath
{
    public ICollection<string> Files { get; } = new List<string>();
    public ICollection<string> Directories { get; } = new List<string>();
}