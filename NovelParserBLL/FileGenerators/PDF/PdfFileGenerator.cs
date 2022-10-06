using NovelParserBLL.FileGenerators.PDF.HTMLQuestPdfBuilder;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Previewer;

namespace NovelParserBLL.FileGenerators.PDF
{
    public class PdfFileGenerator : IFileGenerator<PDFGenerationParams>
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
            GetDocumnet(generationParams).ShowInPreviewer();
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
                    Content = $"<img src=\"{novel.Cover.Name}\"/>",
                    Images = new List<ImageInfo>()
                    {
                        novel.Cover
                    }
                });
            }

            return Document.Create(container =>
            {
                foreach (var item in chaptersWithCover.Values)
                {
                    new BookQuestPdfBuilder(container, item).Build();
                }
            });
        }
    }
}