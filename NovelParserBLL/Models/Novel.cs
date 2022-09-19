namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string? NameRus { get; set; }
        public string? NameEng { get; set; }
        public string? CoverUrl { get; set; }
        public string? CoverPath { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, List<Chapter>>? ChaptersByTranslationTeam { get; set; }
    }
}
