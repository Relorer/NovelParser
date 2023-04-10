namespace NovelParserBLL.JsDTO;

#nullable disable
public class JsNovelInfo
{
    public bool hasStickyPermission { get; set; }
    public object bookmark { get; set; }
    public bool auth { get; set; }
    public string comments_version { get; set; }
    public JsMangaInfo manga { get; set; }
    public JsChaptersInfo chapters { get; set; }
}
#nullable restore