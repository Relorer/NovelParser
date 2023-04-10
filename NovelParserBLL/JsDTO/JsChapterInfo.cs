namespace NovelParserBLL.JsDTO;

#nullable disable
public class JsChapterInfo
{
    public int chapter_id { get; set; }
    public string chapter_slug { get; set; }
    public string chapter_name { get; set; }
    public string chapter_number { get; set; }
    public int chapter_volume { get; set; }
    public int chapter_moderated { get; set; }
    public int chapter_user_id { get; set; }
    public string chapter_expired_at { get; set; }
    public int chapter_scanlator_id { get; set; }
    public string chapter_created_at { get; set; }
    public object status { get; set; }
    public int price { get; set; }
    public string branch_id { get; set; }
    public string username { get; set; }
}
#nullable restore