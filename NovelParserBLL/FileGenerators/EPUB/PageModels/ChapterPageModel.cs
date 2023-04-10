using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public class ChapterPageModel
{
    private readonly ChapterPage _page;
    private readonly string _imagesXhtmlDir;
    private readonly Dictionary<ImageInfo, byte[]?> _imagesBinary;

    public ChapterPageModel(ChapterPage emptyPage, string imagesXhtmlDir)
    {
        _page = emptyPage;
        _imagesXhtmlDir = imagesXhtmlDir;
        Content = string.Empty;
        ChapterNumber = string.Empty;
        ChapterTitle = string.Empty;
        Images = new List<ImageInfo>();
        _imagesBinary = new Dictionary<ImageInfo, byte[]?>();
    }

    public string Content { get; set; }
    public string ChapterNumber { get; set; }
    public string ChapterTitle { get; set;}
    public List<ImageInfo> Images { get; set; }
   
    public async Task<string> GetContent()
    {
        _page.Name = ChapterNumber;
        _page.Title = ChapterTitle;
        
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(Content);

        if (doc.Body == null) 
            return _page.GetPage();

        await FixImagesLinksGetBinaryData(doc);
        _page.Content = await ContentToXhtmlAsync(doc.Body);

        return _page.GetPage();
    }

    public Task<byte[]?> GetImageBinary(ImageInfo image)
    {
        return Task.Run(async () =>
        {
            if (!Images.Contains(image)) return null;
            if (_imagesBinary.ContainsKey(image)) return _imagesBinary[image];
            
            var binary = await image.GetByteArray();
            _imagesBinary.Add(image, binary);
            return binary;
        });
    }

    private async Task FixImagesLinksGetBinaryData(IParentNode html)
    {
        var images = html.QuerySelectorAll("img")
            .Cast<IHtmlImageElement>();

        foreach (var image in images)
        {
            var imageName = Path.GetFileName(image.Source) ?? string.Empty;
            var imageInfo = Images.Find(i =>
                string.Equals(i.Name, imageName, StringComparison.OrdinalIgnoreCase) && i.Exists);
            
            image.Source = string.Empty;

            if (!string.IsNullOrEmpty(imageName) && imageInfo != null)
            {
                var imageBinary = await GetImageBinary(imageInfo);
                if (imageBinary != null)
                {
                    image.Source = HtmlPathHelper.Combine(_imagesXhtmlDir, imageName);
                }
            }
            if (string.IsNullOrEmpty(image.Source))
                image.Remove();
        }
    }

    private static async Task<string> ContentToXhtmlAsync(IMarkupFormattable content)
    {
        var result = (await content.ToXhtmlAsync())
            .Replace("<body>", "", StringComparison.OrdinalIgnoreCase)
            .Replace("</body>", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return result;
    }
}
