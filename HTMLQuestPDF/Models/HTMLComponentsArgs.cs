using HtmlAgilityPack;
using QuestPDF.Helpers;

namespace HTMLQuestPDF.Models
{
    internal class HTMLComponentsArgs
    {
        public PageSize ContainerSize { get; }

        public Func<string, string> GetImagePath { get; }

        public HTMLComponentsArgs(PageSize containerSize, Func<string, string> getImagePath)
        {
            ContainerSize = containerSize;
            GetImagePath = getImagePath;
        }
    }
}
