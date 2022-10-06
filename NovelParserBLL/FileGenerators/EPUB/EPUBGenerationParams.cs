using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators.EPUB
{
    public class EPUBGenerationParams : GenerationParams
    {
        public EPUBGenerationParams(string filePath, Novel novel, string group, string pattern) :
            base(filePath, novel, group, pattern)
        {
        }
    }
}