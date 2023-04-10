using NovelParserBLL.Models;
using NovelParserBLL.Parsers;
using NovelParserBLL.Parsers.kemono;
using NovelParserBLL.Parsers.Libme;

namespace NovelParserBLL.Services
{
    public delegate void SetProgress(int total, int current, string status);

    public class CommonNovelParser
    {
        private const string DefaultAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:101.0) Gecko/20100101 Firefox/101.0";
        private static readonly HttpClientHandler httpClientHandler;
        private static readonly HttpClient httpClient;

        private readonly NovelCacheService novelCacheService;
        private readonly SetProgress setProgress;
        private readonly List<INovelParser> novelParsers = new ();

       
        //ToDo Переделать парсеры
        
        static CommonNovelParser()
        {
            httpClientHandler = new HttpClientHandler();

            //Disable SSL verification
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            httpClient = new HttpClient(httpClientHandler, true);
            httpClient.DefaultRequestHeaders.Add("User-Agent", DefaultAgent);
        }
        public CommonNovelParser(NovelCacheService novelCacheService, SetProgress? setProgress = null)
        {
            this.novelCacheService = novelCacheService;

            this.setProgress = setProgress ?? ((int _, int _, string _) => { });

            novelParsers.Add(new RanobeLibMeParser(this.setProgress, httpClient));
            //novelParsers.Add(new MangaLibMeParser(this.setProgress));
            //novelParsers.Add(new HentaiLibMeParser(this.setProgress));
            //novelParsers.Add(new YaoiLibMeParser(this.setProgress));
            novelParsers.Add(new KemonoParser(this.setProgress));
        }

        public List<ParserInfo> ParserInfos => novelParsers.Select(p => p.ParserInfo).ToList();

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