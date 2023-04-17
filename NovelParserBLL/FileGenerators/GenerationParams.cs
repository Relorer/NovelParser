using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators
{
    public enum FileFormat
    {
        EPUB,
        PDF,
    }

    public abstract class GenerationParams
    {
        public string FilePath { get; init; }
        public Novel Novel { get; init; }
        public string Group { get; init; }
        public string Pattern { get; init; }

        protected GenerationParams(string filePath, Novel novel, string group, string pattern)
        {
            FilePath = filePath;
            Novel = novel;
            Group = group;
            Pattern = pattern;
        }

        public List<Chapter> Chapters => Novel[Group, Pattern];
    }
}