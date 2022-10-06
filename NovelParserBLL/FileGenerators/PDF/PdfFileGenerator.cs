using NovelParserBLL.FileGenerators.PDF.HTMLQuestPdfBuilder;
using NovelParserBLL.Models;
using QuestPDF.Fluent;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal class PdfFileGenerator : IFileGenerator<PDFGenerationParams>
    {
        public Task Generate(PDFGenerationParams generationParams)
        {
            return Task.Run(() =>
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

                var document = Document.Create(container =>
                {
                    foreach (var item in chaptersWithCover.Values)
                    {
                        new BookQuestPdfBuilder(container, item).Build();
                    }
                });

                document.GeneratePdf(generationParams.FilePath);
            });
        }
    }
}