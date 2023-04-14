namespace NovelParserBLL.Parsers.DTO;

#nullable disable
public class SiteTeam
{
    public string name { get; set; }
    public string alt_name { get; set; }
    public string cover { get; set; }
    public string slug { get; set; }
    public int id { get; set; }
    public object branch_id { get; set; }
    public int sale { get; set; }
    public string href { get; set; }
    public SitePivot pivot { get; set; }
}
#nullable restore