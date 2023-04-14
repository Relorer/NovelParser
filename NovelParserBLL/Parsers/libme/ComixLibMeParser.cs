
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using HtmlAgilityPack;
using Jint;
using Jint.Parser.Ast;
using Newtonsoft.Json;
using NovelParserBLL.Parsers.DTO;
using NovelParserBLL.Services.Interfaces;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Parsers.LibMe;

internal abstract class ComicsLibMeParser : BaseLibMeParser
{
    private record ComicInfo(ComicMediaInfo MediaInfo, ComicPage[] Pages);
    private class ServerWithRate
    {
        public ServerWithRate(string server)
        {
            Server = server;
        }

        public int CountImages { get; set; }
        public string Server { get; }
    }

    protected ComicsLibMeParser(SetProgress setProgress, IWebClient webClient) 
        : base(setProgress, webClient)
    {
    }

    protected virtual List<string> Servers => new()
    {
        "https://img3.cdnlib.link/",
        "https://img2.mixlib.me/",
        "https://img4.imgslib.link/",
    };

    public override async Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
    {
        var parsed = 1;
        var nonLoadedChapters = novel[group, pattern].ForLoad(includeImages);
        setProgress(nonLoadedChapters.Count, 0, Resources.ProgressStatusParsing);
        foreach (var item in nonLoadedChapters)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await ParseChapter(novel, item);
            setProgress(nonLoadedChapters.Count, parsed++, Resources.ProgressStatusParsing);
        }

        if (includeImages)
        {
            var allImages = novel[group, pattern].SelectMany(ch => ch.Value.Images).ToList();
            await DownloadImages(allImages, novel.DownloadFolderName, cancellationToken);
        }
    }

    public override string PrepareUrl(string url)
    {
        return SiteDomain + Regex.Match(url.Substring(SiteDomain.Length), @"[^(?|\/)]*").Value;
    }

    public override bool ValidateUrl(string url)
    {
        return url.Length > SiteDomain.Length && url.StartsWith(SiteDomain) && PrepareUrl(url).Length > SiteDomain.Length;
    }

    //todo Возможно, нужно переработать
    private async Task DownloadImages(IReadOnlyCollection<ImageInfo> images, string downloadFolderName, CancellationToken token)
    {
        if (!images.Any()) return;

        var notLoadedImages = images.Where(img => !img.Exists).ToList();
        var serversWithRate = Servers.Select(s => new ServerWithRate(s)).ToList();

        var batchSize = 10;
        setProgress(notLoadedImages.Count, 0, Resources.ProgressStatusImageLoading);
        for (var i = 0; i < notLoadedImages.Count;)
        {
            var batch = notLoadedImages.GetRange(i, Math.Min(batchSize, notLoadedImages.Count - i))
                .Where(img => !img.Exists).ToList();

            foreach (var server in serversWithRate)
            {
                foreach (var imageInfo in batch)
                {
                    var fullPath = Path.Combine(downloadFolderName, imageInfo.Name);
                    var downloadUrl = $"{server.Server + imageInfo.URL}?name={imageInfo.Name}";
                    await DownloadUrl(downloadUrl, fullPath, token);
                }

                server.CountImages += batch.Count;
                if (batch.Count == 0) break;
            }

            if (token.IsCancellationRequested) return;

            i += batchSize;

            setProgress(notLoadedImages.Count, i, Resources.ProgressStatusImageLoading);
            batchSize = Math.Min(50, batchSize + 10);
            serversWithRate.Sort((s1, s2) => s2.CountImages - s1.CountImages);
        }
    }

    private async Task ParseChapter(Novel novel, Chapter chapter)
    {
        if (string.IsNullOrEmpty(chapter.Url)) return;

        var downloadDir = novel.DownloadFolderName;
        var pageContent = await GetPageContent(chapter.Url);
        var pageDoc = await ParseHtmlDocument(pageContent)
                      ?? throw new ApplicationException("Can't parse page content");

        var chapterDoc = await CreateNewDocumentAsync();
        if (chapterDoc.Body == null)
            throw new ApplicationException("Error creating html document.");

        var comicInfo = GetMediaInfo(pageDoc);
        var baseUrl = GetImagesUrl(comicInfo.MediaInfo);

        foreach (var page in comicInfo.Pages)
        {
            var imageRemoteUrl = HtmlPathHelper.Combine(baseUrl,page.Url);
            var imageInfo = new ImageInfo(downloadDir, imageRemoteUrl);
            var imageLocalUrl = HtmlPathHelper.Combine(downloadDir, imageInfo.Name);
            if (!await TryDownloadImage(imageRemoteUrl, imageLocalUrl))
                continue;

            AddImageToDocument(chapterDoc, imageLocalUrl);
            chapter.Images.Add(imageInfo);
        }

        chapter.Content = chapterDoc.Body.InnerHtml;
        chapter.ImagesLoaded = true;
    }
    
    private static ComicInfo GetMediaInfo(IParentNode doc)
    {
        var infoScript = FindScript(doc, "window.__info");
        var pageScript = FindScript(doc, "window.__pg");
        
        var jsEngine = new Engine();
        jsEngine.Execute("var window = {__pg:{},__DATA__:{},__info:{}};");
        jsEngine.Execute(infoScript);
        jsEngine.Execute(pageScript);
        jsEngine.Execute("var jsonPg = JSON.stringify(window.__pg);var jsonInfo = JSON.stringify(window.__info);");
        var pageInfoJson = jsEngine.GetValue("jsonPg").AsString();
        var mediaInfoJson = jsEngine.GetValue("jsonInfo").AsString();
        
        var pages = JsonConvert.DeserializeObject<ComicPage[]>(pageInfoJson)
            ?? Array.Empty<ComicPage>();
        var mediaInfo = JsonConvert.DeserializeObject<ComicMediaInfo>(mediaInfoJson)
            ?? throw new ApplicationException("Cannot parse comic.");

        return new ComicInfo(mediaInfo, pages);
    }
        
    private static string GetImagesUrl(ComicMediaInfo mediaInfo)
    {
        var server = mediaInfo.img.server switch
        {
            "main" => mediaInfo.servers.main,
            "secondary" => mediaInfo.servers.secondary,
            "compress" => mediaInfo.servers.compress,
            "fourth" => mediaInfo.servers.main,
            _ => throw new ApplicationException("Cannot determine server.")
        };
        var sitePath = mediaInfo.img.url.Trim('/');
        var url = HtmlPathHelper.Combine(server, sitePath);
        return url;
    }

    private static void AddImageToDocument(IDocument document, string source)
    {
        if (document.Body == null)
            throw new ApplicationException("Html document have no body section.");

        var div = (IHtmlDivElement)document.CreateElement("div");
        var image = (IHtmlImageElement)document.CreateElement("img");
        SetImageSource(image, source);
        div.AppendChild(image);
        document.Body.AppendChild(div);
    }

    private static Task<IDocument> CreateNewDocumentAsync()
    {
        var context = new BrowsingContext();
        return context.OpenNewAsync();
    }
}