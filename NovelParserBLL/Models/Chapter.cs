namespace NovelParserBLL.Models
{
    public class Chapter
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string? Content { get; set; }
        public string? Number { get; set; }
        public int Index { get; set; }
    }
}
