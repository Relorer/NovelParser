using HTMLQuestPDF.Extensions;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;
using Document = QuestPDF.Fluent.Document;

namespace NovelParserBLL.FileGenerators.PDF
{
    public class PDFFileGenerator : IFileGenerator<PDFGenerationParams>
    {
        public Task Generate(PDFGenerationParams generationParams)
        {
            return Task.Run(() =>
            {
                GetDocument(generationParams).GeneratePdf(generationParams.FilePath);
            });
        }

        public void ShowInPreviewer(PDFGenerationParams generationParams)
        {
            var document = GetDocument(generationParams);
            document.ShowInPreviewer();
            document.GeneratePdf(generationParams.FilePath);
        }
        //todo переделать
        private Document GetDocument(PDFGenerationParams generationParams)
        {
            var novel = generationParams.Novel;

            var chaptersWithCover = new SortedList<float, Chapter>(generationParams.Chapters);
            if (novel.Cover != null)
            {
                chaptersWithCover.Add(-1, new Chapter
                {
                    Name = "Cover",
                    Content = $"<div><a><img src=\"{novel.Cover.Name}\"/></a></div>",
                    Images = new List<ImageInfo>
                    {
                        novel.Cover
                    }
                });
            }

            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            return Document.Create(container =>
            {
                foreach (var item in chaptersWithCover.Values)
                {
                    var getImagePath = (string src) =>
                    {
                        return item.Images.Find(i => i.Name.Equals(src))?.FullPath;
                    };

                    container.HTMLPage(item.Content ?? "", getImagePath, PageSizes.A4, 1, 0.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                }
            });
        }
    }
}