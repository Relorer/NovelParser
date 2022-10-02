using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators
{
    internal interface IFileGenerator
    {
        public Task Generate(string file, Novel novel, string group, string pattern);
    }
}