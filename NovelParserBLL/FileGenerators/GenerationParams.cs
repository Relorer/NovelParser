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
        public FileFormat FileFormat { get; init; }
        public string FilePath { get; init; }
        public Novel Novel { get; init; }
        public string Group { get; init; }
        public string Pattern { get; init; }

        protected GenerationParams(FileFormat fileFormat, string filePath, Novel novel, string group, string pattern)
        {
            FileFormat = fileFormat;
            FilePath = filePath;
            Novel = novel;
            Group = group;
            Pattern = pattern;
        }

        public SortedList<int, Chapter> Chapters => Novel[Group, Pattern];
    }
}