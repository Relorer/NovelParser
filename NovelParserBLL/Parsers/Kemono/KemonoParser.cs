// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using Sayaka.Common;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using NovelParserBLL.Services.Interfaces;


namespace NovelParserBLL.Parsers.kemono;

internal class KemonoParser : INovelParser
{
    private readonly IWebClient _webClient;
    private readonly HtmlParser parser = new();
    private readonly SetProgress setProgress;
    private readonly string urlPattern = @"https:\/\/kemono\.party\/[a-zA-Z]+\/user\/\d*";

    public KemonoParser(SetProgress setProgress, IWebClient webClient)
    {
        this.setProgress = setProgress;
        _webClient = webClient;
    }

    public ParserInfo ParserInfo => new ("https://kemono.party/", "Kemono", "");

    public async Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken token)
    {
        var i = 1;
        var nonLoadedChapters = novel[group, pattern].ForLoad(includeImages);

        setProgress(nonLoadedChapters.Count, 0, Resources.ProgressStatusParsing);
        foreach (var chapter in nonLoadedChapters.Where(chapter => chapter.Url != null))
        {
            var content = await _webClient.GetStringContentAsync(chapter.Url!, token: token);
            if (string.IsNullOrWhiteSpace(content)) continue;
                
            var doc = await parser.ParseDocumentAsync(content, token);
                
            chapter.Content = doc.QuerySelector(".post__body")?.InnerHtml ?? "";
            if (string.IsNullOrEmpty(chapter.Content)) chapter.Name += " <Not loaded>";

            await UpdateImages(chapter, novel.DownloadFolderName);
            UpdateLinks(chapter);

            setProgress(nonLoadedChapters.Count, i++, Resources.ProgressStatusParsing);

            if (token.IsCancellationRequested) return;
        }
    }

    public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken token)
    {
        setProgress(0, 0, Resources.ProgressStatusLoading);
        if (novel == null || string.IsNullOrEmpty(novel.URL)) 
            throw new ArgumentNullException(nameof(novel));

        var doc = await GetHtmlDoc(novel.URL, token);
        if (doc == null) return novel;

        var chapterCount = GetCountChapters(doc);

        if (chapterCount != novel.ChaptersByGroup?.FirstOrDefault().Value.Count)
        {
            novel.ChaptersByGroup = new Dictionary<string, List<Chapter>> {
                { "none", await GetChapters(novel.URL, chapterCount, token) }
            };
        }

        novel.Author = GetName(doc);
        novel.Name = $"{novel.Author}'s Kemono";

        if (!(novel.Cover?.Exists ?? false))
        {
            novel.Cover = await GetImg(GetCoverUrl(doc), novel.DownloadFolderName);
        }

        return novel;
    }

    public string PrepareUrl(string url)
    {
        return Regex.Match(url, urlPattern).Value;
    }

    public bool ValidateUrl(string url)
    {
        return !string.IsNullOrEmpty(PrepareUrl(url));
    }

    private async Task<IHtmlDocument?> GetHtmlDoc(string url, CancellationToken token = default)
    {
        var agent = ProviderFakeUserAgent.Random;
        var content = await _webClient.GetStringContentAsync(url, agent, token);
        if (string.IsNullOrWhiteSpace(content))
            return null;

        return await parser.ParseDocumentAsync(content);
    }

    private async Task<List<Chapter>> GetChapters(string url, int count, CancellationToken token)
    {
        var chapters = new List<Chapter>();

        var j = count;

        for (var i = 0; i < count; i += 25)
        {
            var page = await GetHtmlDoc(url + $"?o={i}", token);
            if (page == null) continue;
            
            foreach (var item in page.QuerySelectorAll(".post-card__heading > a"))
            {
                var chapterUrl = ParserInfo.SiteDomain + item.GetAttribute("href");
                var title = item.TextContent;
                chapters.Add(new Chapter { Name = title, Url = chapterUrl, Content = "", Number = j.ToString() });
                j--;
            }

            setProgress(count, i, Resources.ProgressStatusLoading);
            if (token.IsCancellationRequested) return chapters;
        }

        return chapters;
    }

    private int GetCountChapters(IDocument doc)
    {
        var paginator = doc.QuerySelector(".paginator > small")?.TextContent ?? "of 0";
        return int.Parse(paginator[(paginator.IndexOf("of", StringComparison.Ordinal) + 2)..].Trim());
    }

    private string GetCoverUrl(IDocument doc) => ParserInfo.SiteDomain + doc.QuerySelector(".image-link .fancy-image__image")?.GetAttribute("src");

    private async Task<ImageInfo> GetImg(string url, string folder)
    {
        var result = new ImageInfo(folder, url);

        var binary = await _webClient.GetBinaryContentAsync(url);
        if (!binary.Any())
            return result;

        Directory.CreateDirectory(Path.GetDirectoryName(result.FullPath)!);
        
        await using var stream = File.Create(result.FullPath);
        stream.Write(binary);
        await stream.FlushAsync();

        return result;
    }

    private static string GetName(IParentNode doc) => 
        doc.QuerySelector(".user-header__profile")?.TextContent.Trim() ?? "";

    private async Task UpdateImages(Chapter chapter, string folder)
    {
        if (chapter.Content != null && chapter.Content.Contains("img"))
        {
            var doc = parser.ParseDocument(chapter.Content);

            foreach (var img in doc.QuerySelectorAll("img"))
            {
                var url = img.GetAttribute("src");

                if (string.IsNullOrEmpty(url)) continue;

                var image = await GetImg(ParserInfo.SiteDomain + img.GetAttribute("src"), folder);
                chapter.Images.Add(image);
                img.SetAttribute("src", image.Name);
            }

            chapter.Content = doc.Body?.InnerHtml ?? "";
        }

        chapter.ImagesLoaded = chapter.Images.Exists();
    }

    private void UpdateLinks(Chapter chapter)
    {
        if (chapter.Content == null || !chapter.Content.Contains("fileThumb")) 
            return;

        var doc = parser.ParseDocument(chapter.Content);

        foreach (var a in doc.QuerySelectorAll(".fileThumb"))
        {
            var href = a.GetAttribute("href");
            if (string.IsNullOrEmpty(href)) continue;

            if (href.StartsWith("/data"))
            {
                href = ParserInfo.SiteDomain + href;
            }
            a.SetAttribute("href", href);
        }

        chapter.Content = doc.Body?.InnerHtml ?? "";
    }
}