using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Extensions
{
    internal static class IContainerExtension
    {
        private static Random random = new Random();

        public static IContainer ApplyBlockElementSettings(this IContainer container, string elementName) => elementName switch
        {
            "p" => container.PaddingVertical(6),
            "div" => container,
            "h1" => container.PaddingVertical(12),
            "h2" => container.PaddingVertical(12),
            "h3" => container.PaddingVertical(12),
            "h4" => container.PaddingVertical(12),
            "h5" => container.PaddingVertical(12),
            "h6" => container.PaddingVertical(12),
            _ => container
        };

        public static IContainer Debug(this IContainer container, string name) => container.DebugArea(name, String.Format("#{0:X6}", random.Next(0x1000000)));
    }
}