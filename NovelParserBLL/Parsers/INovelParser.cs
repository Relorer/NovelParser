using NovelParserBLL.Models;

namespace NovelParserBLL.Parsers
{
    public interface INovelParser
    {
        public Task<Novel> ParseAsync(string url);

        public Task ParseAndLoadChapters(Novel novel, string translationTeam);

    }
}