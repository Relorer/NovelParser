using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using Sayaka.Common;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.kemono
{
    internal class KemonoParser : INovelParser
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly HtmlParser parser = new HtmlParser();
        private readonly SetProgress setProgress;
        private readonly string urlPattern = @"https:\/\/kemono\.party\/[a-zA-Z]+\/user\/\d*";
        private IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

        private DefaultHttpRequester requester = new DefaultHttpRequester();

        public KemonoParser(SetProgress setProgress)
        {
            this.setProgress = setProgress;
        }

        public ParserInfo ParserInfo => new ParserInfo("https://kemono.party/", "Kemono", "");

        public Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                int i = 1;
                var nonLoadedChapters = novel[group, pattern].ForLoad(includeImages);

                setProgress(nonLoadedChapters.Count, 0, Resources.ProgressStatusParsing);
                foreach (var chapter in nonLoadedChapters)
                {
                    var doc = await context.OpenAsync(chapter.Url!);

                    var attempt = 1;
                    while (doc.StatusCode != System.Net.HttpStatusCode.OK && attempt < 4)
                    {
                        doc = await context.OpenAsync(chapter.Url!);
                        if (cancellationToken.IsCancellationRequested) return;
                        RefreshContext();
                        await Task.Delay(1000 * attempt++);
                    }

                    chapter.Content = doc.QuerySelector(".post__body")?.InnerHtml ?? "";
                    if (string.IsNullOrEmpty(chapter.Content)) chapter.Name += " <Not loaded>";

                    await UpdateImages(chapter, novel.DownloadFolderName);
                    UpdateLinks(chapter);
                    setProgress(nonLoadedChapters.Count, i++, Resources.ProgressStatusParsing);

                    if (cancellationToken.IsCancellationRequested) return;
                }
            });
        }

        public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken cancellationToken)
        {
            setProgress(0, 0, Resources.ProgressStatusLoading);
            if (novel == null || string.IsNullOrEmpty(novel.URL)) throw new ArgumentNullException(nameof(novel));

            var doc = await context.OpenAsync(novel.URL);

            var chapterCount = GetCountChapters(doc);

            if (chapterCount != novel.ChaptersByGroup?.FirstOrDefault().Value.Count)
            {
                novel.ChaptersByGroup = new Dictionary<string, SortedList<int, Chapter>>() {
                    { "none", await GetChapters(novel!.URL, chapterCount, cancellationToken) }
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

        public void RefreshContext()
        {
            requester.Headers["User-Agent"] = $"--user-agent={ProviderFakeUserAgent.Random}";
            var config = Configuration.Default.With(requester).WithDefaultLoader();
            context = BrowsingContext.New(config);
        }

        public bool ValidateUrl(string url)
        {
            return !string.IsNullOrEmpty(PrepareUrl(url));
        }

        private async Task<SortedList<int, Chapter>> GetChapters(string url, int count, CancellationToken cancellationToken)
        {
            var chapters = new SortedList<int, Chapter>();

            int j = count;

            for (int i = 0; i < count; i += 25)
            {
                var page = await context.OpenAsync(url + $"?o={i}");
                foreach (var item in page.QuerySelectorAll(".post-card__heading > a"))
                {
                    var chapterUrl = ParserInfo.SiteDomen + item.GetAttribute("href");
                    var title = item.TextContent;
                    chapters.Add(j, new Chapter() { Name = title, Url = chapterUrl, Content = "", Number = j.ToString() });
                    j--;
                }

                setProgress(count, i, Resources.ProgressStatusLoading);
                if (cancellationToken.IsCancellationRequested) return chapters;
            }

            return chapters;
        }

        private int GetCountChapters(IDocument doc)
        {
            var paginator = doc.QuerySelector(".paginator > small")?.TextContent ?? "of 0";
            return int.Parse(paginator.Substring(paginator.IndexOf("of") + 2).Trim());
        }

        private string GetCoverUrl(IDocument doc) => ParserInfo.SiteDomen + doc.QuerySelector(".image-link .fancy-image__image")?.GetAttribute("src");

        private async Task<ImageInfo> GetImg(string url, string folder)
        {
            var result = new ImageInfo(folder, url);

            var attempt = 1;

            while (attempt < 4)
            {
                try
                {
                    var bytes = await httpClient.GetByteArrayAsync(url);

                    Directory.CreateDirectory(Path.GetDirectoryName(result.FullPath)!);
                    using var stream = File.Create(result.FullPath);
                    stream.Write(bytes);
                    break;
                }
                catch
                {
                    RefreshContext();
                    attempt++;
                }
            }

            return result;
        }

        private string GetName(IDocument doc) => doc.QuerySelector(".user-header__profile")?.TextContent.Trim() ?? "";

        private async Task UpdateImages(Chapter chapter, string folder)
        {
            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                var doc = parser.ParseDocument(chapter.Content);

                foreach (var img in doc.QuerySelectorAll("img"))
                {
                    string? url = img.GetAttribute("src");

                    if (!string.IsNullOrEmpty(url))
                    {
                        var image = await GetImg(ParserInfo.SiteDomen + img.GetAttribute("src"), folder);
                        chapter.Images.Add(image);
                        img.SetAttribute("src", image.Name);
                    }
                }

                chapter.Content = doc.Body?.InnerHtml ?? "";
            }

            chapter.ImagesLoaded = chapter.Images.Exists();
        }

        private void UpdateLinks(Chapter chapter)
        {
            if (chapter.Content != null && chapter.Content.Contains("fileThumb"))
            {
                var doc = parser.ParseDocument(chapter.Content);

                foreach (var a in doc.QuerySelectorAll(".fileThumb"))
                {
                    string? href = a.GetAttribute("href");

                    if (!string.IsNullOrEmpty(href))
                    {
                        if (href.StartsWith("/data"))
                        {
                            href = ParserInfo.SiteDomen + href;
                        }
                        a.SetAttribute("href", href);
                    }
                }

                chapter.Content = doc.Body?.InnerHtml ?? "";
            }
        }
    }
}