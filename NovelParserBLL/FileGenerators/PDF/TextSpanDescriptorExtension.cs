using QuestPDF.Fluent;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal static class TextSpanDescriptorExtension
    {
        public static TextSpanDescriptor AddFallbackFontFamily(this TextSpanDescriptor textSpan)
        {
            return textSpan.Fallback(y => y.FontFamily("MS Reference Sans Serif")
                           .Fallback(y => y.FontFamily("Segoe UI Emoji")
                           .Fallback(y => y.FontFamily("Microsoft YaHei"))));
        }
    }
}