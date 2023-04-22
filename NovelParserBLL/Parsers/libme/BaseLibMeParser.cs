// ReSharper disable StringLiteralTypo

using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Jint;
using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Parsers.DTO;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.Interfaces;
using NovelParserBLL.Utilities;


namespace NovelParserBLL.Parsers.LibMe;

internal abstract class BaseLibMeParser
{
    private const string InfoScriptStartPattern = @"(?i)^window\.__DATA__(?-i)";

    private readonly IWebClient _webClient;
    protected readonly SetProgress SetProgress;
    
    protected BaseLibMeParser(SetProgress setProgress, IWebClient webClient)
    {
        SetProgress = setProgress;
        _webClient = webClient;
    }

    public abstract string SiteDomain { get; }
    public abstract string SiteName { get; }
    public ParserInfo ParserInfo => new (SiteDomain, SiteName, "https://lib.social/login");

    public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken token)
    {
        SetProgress(0, 0, Resources.ProgressStatusLoading);

        if (!Directory.Exists(novel.DownloadFolderName))
            Directory.CreateDirectory(novel.DownloadFolderName);

        if (string.IsNullOrEmpty(novel.URL)) return novel;

        var content = await _webClient.GetStringContentAsync(novel.URL, token: token);
        var htmlDoc = await ParseHtmlDocument(content, token);
        if (htmlDoc == null) return novel;

        var infoScript = FindScript(htmlDoc, InfoScriptStartPattern);
        if (string.IsNullOrEmpty(infoScript)) return novel;

        var novelInfo = GetNovelInfo(infoScript);
        if (novelInfo == null) return novel;

        var tempNovel = new Novel
        {
            Name = GetNovelName(novelInfo),
            Author = GetNovelAuthor(htmlDoc),
            Description = GetNovelDescription(htmlDoc)
        };

        if (!(novel.Cover?.Exists ?? false))
        {
            var coverUrl = GetNovelCoverUrl(htmlDoc, novelInfo.manga.name);
            tempNovel.Cover = novel.Cover ?? new ImageInfo(novel.DownloadFolderName, coverUrl);
            await DownloadFileAsync(tempNovel.Cover.URL, tempNovel.Cover.FullPath, token);
        }

        tempNovel.ChaptersByGroup = GetChapters(novelInfo);

        novel.Merge(tempNovel);
        novel.Cover = FileHelper.UpdateImageInfo(novel.Cover, novel.DownloadFolderName);

        return novel;
    }

    protected async Task<string> GetPageContent(string url, CancellationToken token = default)
    {
        return await _webClient.GetStringContentAsync(url, token: token);
    }
    protected async Task DownloadFileAsync(string url, string fullPath, CancellationToken token = default)
    {
        var content = await _webClient.GetBinaryContentAsync(url, token: token);
        if (!content.Any()) return;

        await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await fs.WriteAsync(content, token);
        await fs.FlushAsync(token);
    }
    protected async Task<bool> TryDownloadFileAsync(string source, string destination)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;

        await DownloadFileAsync(source, destination);
        return File.Exists(destination);
    }
    protected static void SetImageSource(IHtmlImageElement image, string newSource)
    {
        var filename = Path.GetFileName(newSource);
        image.ClearAttr();
        image.Source = newSource;
        image.SetAttribute("src", newSource);
        image.SetAttribute("name", filename);
        image.SetAttribute("alt", filename);
    }
    protected static async Task<IHtmlDocument?> ParseHtmlDocument(string content, CancellationToken token = default)
    {
        var parser = new HtmlParser();
        return await parser.ParseDocumentAsync(content, token);
    }
    protected static string FindScript(IParentNode doc, string pattern)
    {
        var scripts = doc.QuerySelectorAll("script");

        return scripts.FirstOrDefault(s => 
                Regex.IsMatch(s.InnerHtml.Trim(), pattern))?
            .InnerHtml.Trim()
               ?? throw new ApplicationException($"Cannot find pattern {pattern}.");
    }
    
    private static SiteNovelInfo? GetNovelInfo(string contentScript)
    {
        var jsEngine = new Engine();
        jsEngine.Execute("var window = {__DATA__:{}};");
        jsEngine.Execute(contentScript);
        jsEngine.Execute("var jsonData = JSON.stringify(window.__DATA__);");

        var novelInfoJson = jsEngine.GetValue("jsonData").AsString();
        
        return JsonConvert.DeserializeObject<SiteNovelInfo>(novelInfoJson);
    }
    private static string GetNovelName(SiteNovelInfo info)
    {
        var name = info.manga.engName;
        if (string.IsNullOrWhiteSpace(name)) name = info.manga.rusName;
        if (string.IsNullOrWhiteSpace(name)) name = info.manga.slug;
        return name;
    }
    private static string GetNovelAuthor(IParentNode doc)
    {
        var elems = doc.QuerySelectorAll("div.media-info-list__item");
        var result = elems.FirstOrDefault(e=>string.Equals(e.Children[0].InnerHtml, "Автор", 
                             StringComparison.OrdinalIgnoreCase))
                         ?.Children[1]
                         .Children[0]
                         .InnerHtml
                         .Trim()
                     ?? "(Неизвестно)";
        return result;
    }
    private static string GetNovelDescription(IParentNode doc)
    {
        var elem = doc.QuerySelector(".media-description__text");
        return elem?.Text().Trim() ?? "(Нет описания)";
    }
    private static string GetNovelCoverUrl(IParentNode doc, string title)
    {
        var image = (doc.QuerySelector(@$"img[alt=""{title}""]") 
                    ?? doc.QuerySelector("img.media-header__cover")
                    ?? doc.QuerySelector("div.media-sidebar__cover.paper>img"))
                    as IHtmlImageElement;
        return image?.Source ?? string.Empty;
    }
    private static Dictionary<string, List<Chapter>> GetChapters(SiteNovelInfo siteInfo)
    {
        var result = new Dictionary<string, List<Chapter>>();

        var slug = siteInfo.manga.slug;
        var branches = siteInfo.chapters.branches.Length > 0
            ? siteInfo.chapters.branches
            : new SiteBranch[] { new() { id = "nobranches", name = "none" } };
        
        foreach (var branch in branches)
        {
            var chapters = siteInfo.chapters.list
                .Where(ch => ch.branch_id == branch.id || branch.id == "nobranches");
            var chaptersList = chapters.Select(chapter => 
                new Chapter 
                    { 
                        Name = chapter.chapter_name, 
                        Number = chapter.chapter_number, 
                        Volume = chapter.chapter_volume, 
                        Url = BuildChapterUrl(slug, chapter.chapter_volume, chapter.chapter_number)
                    }).ToList();

            result.Add(branch.id, chaptersList.SortChapters());
        }
        
        return result;
    }

    private static string BuildChapterUrl(string slug, int volume, string number)
    {
        return $@"https://ranobelib.me/{slug}/v{volume}/c{number}";
    }
}