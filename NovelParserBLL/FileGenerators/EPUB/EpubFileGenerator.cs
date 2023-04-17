using NovelParserBLL.Extensions;
using NovelParserBLL.FileGenerators.EPUB.PageModels;
using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators.EPUB;

internal class EpubFileGenerator : IFileGenerator<EPUBGenerationParams>
{
    private const string ImagesXhtmlDir = "Images";
    private readonly ChapterPage _emptyChapterPage;

    public EpubFileGenerator()
    {
        _emptyChapterPage = new ChapterPage(new ChapterPageTemplate());
    }

    public async Task Generate(EPUBGenerationParams generationParams)
    {
        var novel = generationParams.Novel
                    ?? throw new ApplicationException("Novel object not set.");
        var chapters = generationParams.Chapters;

        var epubDoc = new EpubDocument(generationParams.FilePath, ImagesXhtmlDir);

        novel.Name = !string.IsNullOrWhiteSpace(novel.Name) ? novel.Name : "(без названия)";
        novel.Author = !string.IsNullOrWhiteSpace(novel.Author) ? novel.Name : "(нет автора)";
        novel.Description = !string.IsNullOrWhiteSpace(novel.Description) ? novel.Name : "(нет описания)";

        AddNovelInfo(epubDoc, novel);
        await AddCoverPage(epubDoc, novel.Name, novel.Cover);
        await AddChapters(epubDoc, chapters);
        await epubDoc.GenerateDocumentAsync();
    }

    private static void AddNovelInfo(EpubDocument epubDoc, Novel novel)
    {
        epubDoc.AddAuthor(novel.Author);
        epubDoc.AddTitle(novel.Name);
        epubDoc.AddDescription(novel.Description);
    }

    private static async Task AddCoverPage(EpubDocument epubDoc, string novelName, ImageInfo? cover)
    {
        var coverPageBuilder = new CoverPageModel(new CoverPageTemplate(), ImagesXhtmlDir)
        {
            Title = novelName
        };

        if (cover != null && !string.IsNullOrEmpty(cover.Name))
        {
            var coverBinary = await cover.GetByteArray();
            coverPageBuilder.CoverImageName = cover.Name;
            if (coverBinary != null)
                await epubDoc.AddImageAsync(cover.Name, coverBinary);
        }

        var coverPage = coverPageBuilder.GetContent();
        epubDoc.AddPage("Cover", "Постер", coverPage);
    }

    private async Task AddChapters(EpubDocument epubDoc, List<Chapter> chapters)
    {
        var volumes = chapters.VolumesCount();

        foreach (var chapter in chapters.SortChapters())
        {
            var chapNum = (volumes <= 1 ? "" : $"Том {chapter.Volume}, ")
                            + $"Глава {chapter.Number}";

            var page = new ChapterPageModel(_emptyChapterPage, ImagesXhtmlDir)
            {
                ChapterNumber = chapNum,
                ChapterTitle = chapter.Name ?? string.Empty,
                Content = chapter.Content ?? string.Empty,
                Images = chapter.Images
            };
            var pageContent = await page.GetContent();
            var pageName = $"chapter_v{chapter.Volume}_{chapter.Number}";
            var label = page.ChapterNumber
                        + (string.IsNullOrEmpty(page.ChapterTitle) ? "" : $" - {page.ChapterTitle}");

            epubDoc.AddPage(pageName, label, pageContent);
            await AddPageImages(epubDoc, page);
        }
    }

    private static async Task AddPageImages(EpubDocument doc, ChapterPageModel page)
    {
        foreach (var image in page.Images)
        {
            var binary = await page.GetImageBinary(image);
            if (binary != null)
                await doc.AddImageAsync(image.Name, binary);
        }
    }
}