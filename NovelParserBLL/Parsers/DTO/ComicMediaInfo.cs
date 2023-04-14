namespace NovelParserBLL.Parsers.DTO;

public class ComicMediaInfo
{
    public int page { get; set; }
    public ComicMedia media { get; set; }
    public object bookmark { get; set; }
    public ComicCurrent current { get; set; }
    public object next { get; set; }
    public object prev { get; set; }
    public ComicImg img { get; set; }
    public ComicServers servers { get; set; }
}
