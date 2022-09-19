using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators
{
    public enum FileFormatForGenerator
    {
        EPUB,
        PDF,
    }

    internal interface IFileGenerator
    {
        public void Generate(string file, Novel novel, string translationTeam);
    }
}
