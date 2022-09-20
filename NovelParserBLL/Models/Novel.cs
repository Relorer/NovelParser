using System.Drawing;

namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string NameRus { get; }
        public string NameEng { get; }
        public string CoverUrl { get; }
        public string Author { get; }
        public string Description { get; }
        public byte[]? Cover { get; set; }
        public Dictionary<string, SortedList<int, Chapter>>? ChaptersByTranslationTeam { get; set; }

        public Novel(string nameRus, string nameEng, string coverUrl, string author, string description)
        {
            NameRus = nameRus;
            NameEng = nameEng;
            CoverUrl = coverUrl;
            Author = author;
            Description = description;
        }
    }
}
