using NovelParserBLL.Models;
using QuestPDF.Fluent;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal class PdfFileGenerator : IFileGenerator
    {
        public Task Generate(string file, Novel novel, string group, string pattern)
        {
            return Task.Run(() =>
            {
                var chaptersWithCover = new SortedList<int, Chapter>(novel[group, pattern]);
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

                document.GeneratePdf(file);
            });
        }
    }
}