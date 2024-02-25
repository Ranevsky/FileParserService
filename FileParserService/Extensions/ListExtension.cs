namespace FileParserService.Extensions;

public static class ListExtension
{
    public static TValue GetRandom<TValue>(this IList<TValue> collection)
    {
        var randomIndex = Random.Shared.Next(collection.Count);

        return collection[randomIndex];
    }
}