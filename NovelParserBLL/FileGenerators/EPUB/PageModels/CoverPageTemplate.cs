using NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;

namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public class CoverPageTemplate : BaseTemplate, ICoverPageTemplate
{
    public CoverPageTemplate() : base("CoverTemplate.xhtml")
    {
        TitlePlaceholder = "{title}";
        ImageTagLimiters = new Limiters("{start}", "{end}");
        ImagePathPlaceholder = "{cover}";
    }

    public string TitlePlaceholder { get; }
    public string ImagePathPlaceholder { get; }
    public Limiters ImageTagLimiters { get; }
}
