using AngleSharp.Xhtml;
using AngleSharp;

namespace NovelParserBLL.Extensions;

public static class MarkupFormattableExtension
{
    public static async Task<string> ToXhtmlAsync(this IMarkupFormattable html)
    {
        await using var sw = new StringWriter();
        await Task.Run(()=>html.ToHtml(sw, new XhtmlMarkupFormatter(true)));
        return sw.ToString();
    }
}
