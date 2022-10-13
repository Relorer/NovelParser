using HtmlAgilityPack;
using HTMLQuestPDF.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF
{
    internal static class StyleSettings
    {
        // `text` auxiliary tag
        public static readonly string[] LineElements = new string[] {
            "a",
            "b",
            "br",
            "i",
            "s",
            "small",
            "space",
            "strike",
            "tbody",
            "td",
            "th",
            "thead",
            "tr",
            "u",
        };

        public static readonly string[] BlockElements = new string[] {
            "#document",
            "div",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "img",
            "li",
            "ol",
            "p",
            "table",
            "ul",
        };

        public static readonly TextStyle TextStyle = TextStyle.Default
            .Fallback(y => y.FontFamily("MS Reference Sans Serif")
            .Fallback(y => y.FontFamily("Segoe UI Emoji")
            .Fallback(y => y.FontFamily("Microsoft YaHei"))));

        public static TextSpanAction GetTextStyles(this HtmlNode element)
        {
            return (span) => span.Style(element.GetTextStyle());
        }

        public static TextStyle GetTextStyle(this HtmlNode element)
        {
            return element.Name.ToLower() switch
            {
                "h1" => TextStyle.Default.FontSize(32).Bold(),
                "h2" => TextStyle.Default.FontSize(28).Bold(),
                "h3" => TextStyle.Default.FontSize(24).Bold(),
                "h4" => TextStyle.Default.FontSize(20).Bold(),
                "h5" => TextStyle.Default.FontSize(16).Bold(),
                "h6" => TextStyle.Default.FontSize(12).Bold(),
                "b" => TextStyle.Default.Bold(),
                "i" => TextStyle.Default.Italic(),
                "small" => TextStyle.Default.Light(),
                "strike" => TextStyle.Default.Strikethrough(),
                "s" => TextStyle.Default.Strikethrough(),
                "u" => TextStyle.Default.Underline(),
                _ => TextStyle.Default
            };
        }
    }
}