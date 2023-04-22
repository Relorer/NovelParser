using NovelParserBLL.Extensions;
using NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public class CoverPageModel
{
    private readonly ICoverPageTemplate template;
    private readonly string _imagesXhtmlDir;
    public CoverPageModel(ICoverPageTemplate coverPageTemplate, string imagesXhtmlDir)
    {
        template = coverPageTemplate;
        _imagesXhtmlDir = imagesXhtmlDir;
        Title = string.Empty;
        CoverImageName = string.Empty;
    }
    public string Title { get; set; }
    public string CoverImageName { get; set; }

    public string GetContent()
    {
        var coverPage = template.Template;
        coverPage = coverPage.Replace(template.TitlePlaceholder, Title);

        if (string.IsNullOrWhiteSpace(CoverImageName))
        {
            coverPage = coverPage
                .DeleteSubstring(template.ImageTagLimiters.Start, template.ImageTagLimiters.End);
        }
        else
        {
            var imagePath = HtmlPathHelper.Combine(_imagesXhtmlDir, CoverImageName);
            coverPage = coverPage.DeleteSubstring(template.ImageTagLimiters.Start)
                .DeleteSubstring(template.ImageTagLimiters.End)
                .Replace(template.ImagePathPlaceholder, imagePath);
        }

        return coverPage;
    }


}
