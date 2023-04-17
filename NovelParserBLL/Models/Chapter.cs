namespace NovelParserBLL.Models;

public class Chapter
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Number { get; set; }
    public int Volume { get; set; }
    public string? Content { get; set; }
    public List<ImageInfo> Images { get; set; } = new ();
    public bool ImagesLoaded { get; set; }
}