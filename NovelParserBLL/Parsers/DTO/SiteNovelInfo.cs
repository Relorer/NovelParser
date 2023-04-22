namespace NovelParserBLL.Parsers.DTO;

#nullable disable
public class SiteNovelInfo
{
    public bool hasStickyPermission { get; set; }
    public object bookmark { get; set; }
    public bool auth { get; set; }
    public string comments_version { get; set; }
    public SiteMangaInfo manga { get; set; }
    public SiteChaptersInfo chapters { get; set; }
}
#nullable restore