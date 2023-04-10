namespace NovelParserBLL.FileGenerators.EPUB.PageModels.Interfaces;

public record Limiters(string Start, string End);
public interface ICoverPageTemplate
{
    public string Template { get; }
    public string TitlePlaceholder { get; }
    public string ImagePathPlaceholder { get; }
    public Limiters ImageTagLimiters { get; }
}
