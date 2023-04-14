using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using NovelParserBLL.Services.Interfaces;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Parsers.LibMe;

internal class RanobeLibMeParser : BaseLibMeParser
{
    public RanobeLibMeParser(SetProgress setProgress, IWebClient webClient) 
        : base(setProgress, webClient) { }

    public override string SiteDomain => "https://ranobelib.me/";
    public override string SiteName => "RanobeLib.me";

    public override async Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken token)
    {
        var parsed = 1;
        var nonLoadedChapters = novel[group, pattern].ForLoad(includeImages);

        setProgress(nonLoadedChapters.Count, 0, Resources.ProgressStatusParsing);
        
        foreach (var item in nonLoadedChapters)
        {
            if (token.IsCancellationRequested) return;

            await ParseChapter(novel, item, includeImages);
            
            setProgress(nonLoadedChapters.Count, parsed++, Resources.ProgressStatusParsing);
        }
    }

    public override string PrepareUrl(string url)
    {
        return SiteDomain + Regex.Match(url[SiteDomain.Length..], @"[^(?|\/)]*").Value;
    }

    public override bool ValidateUrl(string url)
    {
        return url.Length > SiteDomain.Length 
               && url.StartsWith(SiteDomain) 
               && PrepareUrl(url).Length > SiteDomain.Length;
    }

    private async Task ParseChapter(Novel novel, Chapter chapter, bool includeImages)
    {
        if (string.IsNullOrEmpty(chapter.Url)) return;

        var pageContent = await GetPageContent(chapter.Url);

        var pageDoc = await ParseHtmlDocument(pageContent)
                      ?? throw new ApplicationException("Can't parse page content");

        var chapterContent = pageDoc.QuerySelector(".reader-container")
                             ?? throw new ApplicationException("Can't parse page content");

        var images = chapterContent.QuerySelectorAll("img")
            .Cast<IHtmlImageElement>();
        
        foreach (var image in images)
        {
            if (includeImages)
            {
                var imageInfo = await ProcessImage(image, novel.DownloadFolderName);
                if (imageInfo != null)
                    chapter.Images.Add(imageInfo);
            }
            else
                image.Remove();
        }

        chapter.Content = chapterContent.InnerHtml;
        chapter.ImagesLoaded = includeImages;
    }

    private async Task<ImageInfo?> ProcessImage(IHtmlImageElement image, string downloadDir)
    {
        var filename = $"{Guid.NewGuid()}.png";
        var fullPath = Path.Combine(downloadDir, filename);
        var source = image.GetAttribute("data-src") ?? image.Source ?? string.Empty;
        if (!await TryDownloadFileAsync(source, fullPath))
        {
            image.Remove();
            return null;
        }

        SetImageSource(image, fullPath);
        var imageInfo = new ImageInfo(fullPath, filename, image.Source ?? string.Empty);
        
        return FileHelper.UpdateImageInfo(imageInfo, downloadDir);
    }
}