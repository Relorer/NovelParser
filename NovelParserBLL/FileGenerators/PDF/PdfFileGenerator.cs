using HTMLQuestPDF.Extensions;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;

using Document = QuestPDF.Fluent.Document;

namespace NovelParserBLL.FileGenerators.PDF;

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

        var chaptersWithCover = new List<Chapter>(generationParams.Chapters);
        if (novel.Cover != null)
        {
            chaptersWithCover.Add(new Chapter
            {
                Name = "Cover",
                Volume = 0,
                Number = "0",
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
            foreach (var item in chaptersWithCover.SortChapters())
            {
                string GetImagePath(string src)
                {
                    return item.Images.Find(i => i.Name.Equals(src))?.FullPath ?? string.Empty;
                }

                container.HTMLPage(item.Content ?? "", GetImagePath, PageSizes.A4, 1, 0.5f, QuestPDF.Infrastructure.Unit.Centimetre);
            }
        });
    }
}