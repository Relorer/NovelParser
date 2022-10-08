using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF
{
    internal static class IContainerExtension
    {
        public static IContainer ApplyBlockElementSettings(this IContainer container, string elementName) => elementName switch
        {
            "p" => container.PaddingVertical(6),
            "div" => container.PaddingVertical(12),
            "h1" => container.PaddingVertical(12),
            "h2" => container.PaddingVertical(12),
            "h3" => container.PaddingVertical(12),
            "h4" => container.PaddingVertical(12),
            "h5" => container.PaddingVertical(12),
            "h6" => container.PaddingVertical(12),
            _ => container
        };
    }
}