using HTMLQuestPDF;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;

namespace NovelParserBLL.FileGenerators.PDF
{
    public class PDFFileGenerator : IFileGenerator<PDFGenerationParams>
    {
        public Task Generate(PDFGenerationParams generationParams)
        {
            return Task.Run(() =>
            {
                GetDocumnet(generationParams).GeneratePdf(generationParams.FilePath);
            });
        }

        public void ShowInPreviewer(PDFGenerationParams generationParams)
        {
            var document = GetDocumnet(generationParams);
            document.ShowInPreviewer();
            document.GeneratePdf(generationParams.FilePath);
        }

        private Document GetDocumnet(PDFGenerationParams generationParams)
        {
            var novel = generationParams.Novel;

            var chaptersWithCover = new SortedList<int, Chapter>(generationParams.Chapters);
            if (novel.Cover != null)
            {
                chaptersWithCover.Add(-1, new Chapter()
                {
                    Name = "Cover",
                    Content = $"<div><a><img src=\"{novel.Cover.Name}\"/></a></div>",
                    Images = new List<ImageInfo>()
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