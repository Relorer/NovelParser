using NovelParserBLL.Models;
using NovelParserBLL.Services;

namespace NovelParserBLL.FileGenerators.EPUB
{
    public class EPUBGenerationParams : GenerationParams
    {
        public EPUBGenerationParams(FileFormat fileFormat, string filePath, Novel novel, string group, string pattern) :
            base(fileFormat, filePath, novel, group, pattern)
        {
        }
    }
}