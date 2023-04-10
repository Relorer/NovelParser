namespace NovelParserBLL.Utilities;

public static class HtmlPathHelper
{
    public static string Combine(params string[] paths)
    {
        return Path.Combine(paths).Replace("\\", "/");
    }
}
