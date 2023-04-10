using net.vieapps.Components.Utility.Epub;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.FileGenerators.EPUB;

public class EpubDocument
{
    private readonly Document _epubDocument;
    private readonly string _fullPathToEpub;
    private readonly string _imagesDir;
    private int _pagePlayOrder;
    public EpubDocument(string fullPathToEpubFile, string imagesDirXhtml)
    {
        _imagesDir = imagesDirXhtml;
        _pagePlayOrder = 1;
        _epubDocument = new Document();
        _fullPathToEpub = fullPathToEpubFile;
        _epubDocument.AddMetaItem("dc:language", "en");
    }

    public void AddAuthor(string? author)
    {
        if (!string.IsNullOrEmpty(author))
            _epubDocument.AddAuthor(author);
    }

    public void AddTitle(string? title)
    {
        if (!string.IsNullOrEmpty(title))
            _epubDocument.AddTitle(title);
    }

    public void AddDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description))
            _epubDocument.AddDescription(description);
    }

    public Task AddImageAsync(string imageName, byte[] imageBinary)
    {
        return Task.Run(() =>
        {
            var path = HtmlPathHelper.Combine(_imagesDir, imageName);
            var tag = _epubDocument.AddImageData(path, imageBinary);
            _epubDocument.AddMetaItem(imageName, tag);
        });
    }

    /// <summary>
    /// Добавить страницу в контейнер. Страницы добавляются в порядке вызовов метода
    /// </summary>
    /// <param name="pageName">Имя файла в контейнере (без расширения)</param>
    /// <param name="label">Метка в оглавлении</param>
    /// <param name="content">Содержимое xhtml файла страницы</param>
    public void AddPage(string pageName, string label, string content)
    {
        var pageXhtmlName = $"{pageName}.xhtml";
        _epubDocument.AddXhtmlData(pageXhtmlName, content);
        _epubDocument.AddNavPoint(label, pageXhtmlName, _pagePlayOrder);
        _pagePlayOrder++;
    }

    public Task GenerateDocumentAsync()
    {
        return Task.Run(() => { _epubDocument.Generate(_fullPathToEpub); });
    }
}
