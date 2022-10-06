using HtmlAgilityPack;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NovelParserBLL.FileGenerators.PDF.HTMLQuestPdfBuilder
{
    internal class BookQuestPdfBuilder : HTMLQuestPdfBuilder
    {
        public BookQuestPdfBuilder(IDocumentContainer container, Chapter chapter) : base(container, chapter)
        {
        }

        protected override IContainer? GetContainerElement(string elementName, ColumnDescriptor column)
        {
            return elementName switch
            {
                "p" => column.Item().PaddingVertical(6),
                "div" => column.Item().PaddingVertical(2),
                "h1" => column.Item().PaddingVertical(12),
                "h2" => column.Item().PaddingVertical(10),
                "h3" => column.Item().PaddingVertical(8),
                "h4" => column.Item().PaddingVertical(6),
                "h5" => column.Item().PaddingVertical(4),
                "h6" => column.Item().PaddingVertical(2),
                _ => null
            };
        }

        protected override IContainer GetContentContainer(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            return page.Content()
                .PaddingVertical(0.5f, Unit.Centimetre)
                .PaddingHorizontal(1f, Unit.Centimetre);
        }

        protected override void RenderImg(ColumnDescriptor column, HtmlNode node)
        {
            var src = node.GetAttributeValue("src", "");
            var fullPath = chapter.Images.Find(i => i.Name.Equals(src))?.FullPath;
            var con = column.Item().PaddingVertical(4);
            if (fullPath == null) con.Image(Placeholders.Image(200, 100), ImageScaling.FitArea);
            else con.Image(fullPath, ImageScaling.FitArea);
        }
    }
}