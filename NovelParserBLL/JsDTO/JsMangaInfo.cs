namespace NovelParserBLL.JsDTO;

#nullable disable
public class JsMangaInfo
{
    public int id { get; set; }
    public string name { get; set; }
    public string rusName { get; set; }
    public string engName { get; set; }
    public string slug { get; set; }
    public int status { get; set; }
    public int chapters_count { get; set; }
    public string[] altNames { get; set; }
}
#nullable restore