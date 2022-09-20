namespace NovelParserBLL.Models
{
    public class Chapter
    {
        public string Name { get; }
        public string Url { get; }
        public string Number { get; }
        public int Index { get; }
        public string Content { get; set; }
        public bool ImagesLoaded { get; set; }

        public Chapter(string name, string url, string content, string number, int index)
        {
            Name = name;
            Url = url;
            Content = content;
            Number = number;
            Index = index;
        }
    }
}
