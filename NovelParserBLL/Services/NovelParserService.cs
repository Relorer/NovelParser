using NovelParserBLL.Models;
using NovelParserBLL.Parsers;
using NovelParserBLL.Parsers.Kemono;
using NovelParserBLL.Parsers.Ranobelib;

namespace NovelParserBLL.Services
{
    public enum NovelParserType
    {
        ranobelibme,
        kemono
    }

    public static class NovelParserService
    {
        private static Dictionary<NovelParserType, INovelParser> novelParsers = new Dictionary<NovelParserType, INovelParser>()
        {
            {NovelParserType.ranobelibme, new RanobelibParser() },
            {NovelParserType.kemono, new KemonoParser() },
        };

        public static Task ParseAndLoadChapters(Novel novel, SortedList<int, Chapter> chapters, bool includeImages, Action<int, int> setProgress, CancellationToken cancellationToken)
        {
            return GetParser(novel.URL)!.ParseAndLoadChapters(novel, chapters, includeImages, setProgress, cancellationToken);
        }

        public static Task<Novel?> ParseAsync(string novelUrl, CancellationToken cancellationToken)
        {
            return GetParser(novelUrl)!.ParseAsync(novelUrl, cancellationToken);
        }

        public static bool ValidateUrl(string url)
        {
            return GetParser(url) != null;
        }

        private static INovelParser? GetParser(string url)
        {
            foreach (var item in novelParsers)
            {
                if (item.Value.ValidateUrl(url)) return item.Value;
            }

            return null;
        }
    }
}
