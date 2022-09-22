using NovelParserBLL.Models;

namespace NovelParserBLL.Parsers
{
    internal interface INovelParser
    {
        Task ParseAndLoadChapters(Novel novel, SortedList<int, Chapter> chapters, bool includeImages, Action<int, int> setProgress, CancellationToken cancellationToken);
        Task<Novel?> ParseAsync(string novelUrl, CancellationToken cancellationToken);
        bool ValidateUrl(string url);
    }
}