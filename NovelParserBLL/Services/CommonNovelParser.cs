using NovelParserBLL.Models;
using NovelParserBLL.Parsers;
using NovelParserBLL.Parsers.kemono;
using NovelParserBLL.Parsers.LibMe;

namespace NovelParserBLL.Services;

public delegate void SetProgress(int total, int current, string status);

public class CommonNovelParser
{
    private readonly NovelCacheService novelCacheService;
    private readonly List<INovelParser> novelParsers = new ();

       
    //ToDo Переделать парсеры
        
    public CommonNovelParser(NovelCacheService novelCacheService, SetProgress? setProgress = null)
    {
        var webClient = new WebClient();
            
        this.novelCacheService = novelCacheService;
        var setProgressAction = setProgress ?? ((int _, int _, string _) => { });

        novelParsers.Add(new RanobeLibMeParser(setProgressAction, webClient));
        novelParsers.Add(new KemonoParser(setProgressAction, webClient));
        novelParsers.Add(new MangaLibMeParser(setProgressAction, webClient));
        novelParsers.Add(new HentaiLibMeParser(setProgressAction, webClient));
        novelParsers.Add(new YaoiLibMeParser(setProgressAction, webClient));
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
        return novelParsers.FirstOrDefault(item => item.ValidateUrl(url));
    }
}