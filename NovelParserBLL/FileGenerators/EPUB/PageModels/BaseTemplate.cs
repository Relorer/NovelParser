namespace NovelParserBLL.FileGenerators.EPUB.PageModels;

public abstract class BaseTemplate
{
    private readonly Lazy<string> lazyTemplate;

    protected BaseTemplate(string templateResource)
    {
        lazyTemplate = new Lazy<string>(() =>
            ResourceReader.GetResourceData(templateResource).Trim());
    }
    public string Template => lazyTemplate.Value;
}
