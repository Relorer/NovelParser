using NovelParserBLL.Models;

namespace NovelParserBLL.Parsers
{
    internal interface INovelParser
    {
        public Task<Novel> ParseAsync(string url);

        public Task ParseAndLoadChapters(Novel novel);

    }
}