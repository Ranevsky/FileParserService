using System.Text;

namespace FileParserService.Extensions;

public static class SpanExtension
{
    public static ReadOnlySpan<char> RemoveTag(this ReadOnlySpan<char> span, string startTag, string endTag)
    {
        var result = new StringBuilder(span.Length);
        while (true)
        {
            var startIndex = span.IndexOf(startTag);
            if (startIndex == -1)
            {
                result.Append(span);

                break;
            }

            result.Append(span[..startIndex]);
            span = span[startIndex..];

            var endIndex = span.IndexOf(endTag);
            if (endIndex == -1)
            {
                break;
            }

            span = span[(endIndex + endTag.Length)..];
        }

        return result.ToString().AsSpan();
    }
}