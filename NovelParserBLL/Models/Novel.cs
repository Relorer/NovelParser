using System.Drawing;

namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string? NameRus { get; set; }
        public string? NameEng { get; set; }
        public string? CoverUrl { get; set; }
        public byte[]? Cover { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, SortedList<int, Chapter>>? ChaptersByTranslationTeam { get; set; }
    }
}
