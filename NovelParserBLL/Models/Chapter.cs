namespace NovelParserBLL.Models
{
    public class Chapter
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Number { get; set; }
        public string? Content { get; set; }
        public Dictionary<string, byte[]> Images { get; set; } = new Dictionary<string, byte[]>();
        public bool ImagesLoaded { get; set; }

    }
}
