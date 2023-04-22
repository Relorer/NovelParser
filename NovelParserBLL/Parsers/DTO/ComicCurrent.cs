namespace NovelParserBLL.Parsers.DTO;

#nullable disable
public class ComicCurrent
{
    public int id { get; set; }
    public int volume { get; set; }
    public string number { get; set; }
    public int index { get; set; }
    public object status { get; set; }
    public int price { get; set; }
}
#nullable restore