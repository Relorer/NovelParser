using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators
{
    internal interface IFileGenerator
    {
        public void Generate(string file, Novel novel);
    }
}
