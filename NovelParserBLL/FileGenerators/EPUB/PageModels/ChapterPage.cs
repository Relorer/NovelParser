using NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;

namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public class ChapterPage
{
    private readonly IChapterPageTemplate _template;
    public ChapterPage(IChapterPageTemplate template)
    {
        _template = template;
        Title = string.Empty;
        Name = string.Empty;
        Content = string.Empty;
    }
    public string Title { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }

    public string GetPage()
    {
        return _template.Template.Replace(_template.TitlePlaceholder, Title)
            .Replace(_template.ContentPlaceholder, Content)
            .Replace(_template.NamePlaceholder, Name)
            .Trim();
    }
}
