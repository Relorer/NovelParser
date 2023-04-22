using NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;

namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public class ChapterPageTemplate : BaseTemplate, IChapterPageTemplate
{
    public ChapterPageTemplate() : base("PageTemplate.xhtml")
    {
        TitlePlaceholder = "{title}";
        NamePlaceholder = "{number}";
        ContentPlaceholder = "{content}";
    }
    public string TitlePlaceholder { get; }
    public string NamePlaceholder { get; }
    public string ContentPlaceholder { get; }
}
