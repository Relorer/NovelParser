namespace NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;

public interface IChapterPageTemplate
{
    public string Template { get; }
    public string TitlePlaceholder { get; }
    public string NamePlaceholder { get; }
    public string ContentPlaceholder { get; }
}
