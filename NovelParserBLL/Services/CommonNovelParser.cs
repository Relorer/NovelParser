using NovelParserBLL.Models;
using NovelParserBLL.Parsers;
using NovelParserBLL.Parsers.kemono;
using NovelParserBLL.Parsers.libme;

namespace NovelParserBLL.Services
{
    public delegate void SetProgress(int total, int current);

    public class CommonNovelParser
    {
        private readonly NovelCacheService novelCacheService;
        private readonly SetProgress setProgress;
        private readonly List<INovelParser> novelParsers = new List<INovelParser>();

        public CommonNovelParser(NovelCacheService novelCacheService, SetProgress? setProgress = null)
        {
            this.novelCacheService = novelCacheService;

            this.setProgress = setProgress ?? ((int _, int _) => { });

            novelParsers.Add(new RanobelibParser(this.setProgress));
            novelParsers.Add(new KemonoParser(this.setProgress));
            novelParsers.Add(new MangaLibMeParser(this.setProgress));
            novelParsers.Add(new HentaiLibMeParser(this.setProgress));
            novelParsers.Add(new YaoiLibMeParser(this.setProgress));
        }

        public async Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            try
            {
                await GetParser(novel.URL!)!.LoadChapters(novel, group, pattern, includeImages, cancellationToken);
            }
            finally
            {
                novelCacheService.SaveNovelToFile(novel);
            }
        }

        public async Task<Novel> ParseCommonInfo(string novelUrl, CancellationToken cancellationToken)
        {
            var parser = GetParser(novelUrl)!;
            var url = parser.PrepareUrl(novelUrl);

            Novel novel = novelCacheService.TryGetNovelFromFile(url) ?? new Novel() { URL = url };
            novel = await parser.ParseCommonInfo(novel, cancellationToken);

            novelCacheService.SaveNovelToFile(novel);

            return novel;
        }

        public bool ValidateUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && GetParser(url) != null;
        }

        private INovelParser? GetParser(string url)
        {
            foreach (var item in novelParsers)
            {
                if (item.ValidateUrl(url)) return item;
            }

            return null;
        }
    }
}