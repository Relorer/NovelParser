using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
using Sayaka.Common;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.kemono
{
    internal class KemonoParser : INovelParser
    {
        private readonly string kemonoUrl = "https://kemono.party/";
        private readonly string urlPattern = @"https:\/\/kemono\.party\/[a-zA-Z]+\/user\/\d*";

        private readonly HtmlParser parser = new HtmlParser();
        private readonly HttpClient httpClient = new HttpClient();

        private readonly SetProgress setProgress;

        public KemonoParser(SetProgress setProgress)
        {
            this.setProgress = setProgress;
        }

        private IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private DefaultHttpRequester requester = new DefaultHttpRequester();

        public Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                int i = 1;
                var nonLoadedChapters = novel[group, pattern].Where(ch => string.IsNullOrEmpty(ch.Value.Content) || ch.Value.ImagesLoaded ^ includeImages).ToList();

                foreach (var chapter in nonLoadedChapters)
                {
                    var doc = await context.OpenAsync(chapter.Value.Url!);

                    var attempt = 1;
                    while (doc.StatusCode != System.Net.HttpStatusCode.OK && attempt < 4)
                    {
                        doc = await context.OpenAsync(chapter.Value.Url!);
                        if (cancellationToken.IsCancellationRequested) return;
                        RefreshContext();
                        await Task.Delay(1000 * attempt++);
                    }

                    chapter.Value.Content = doc.QuerySelector(".post__body")?.InnerHtml ?? "";
                    if (string.IsNullOrEmpty(chapter.Value.Content)) chapter.Value.Name += " <Not loaded>";

                    await UpdateImages(chapter.Value, includeImages);
                    setProgress(nonLoadedChapters.Count, i++);

                    if (cancellationToken.IsCancellationRequested) return;
                }
            });
        }

        public void RefreshContext()
        {
            requester.Headers["User-Agent"] = $"--user-agent={ProviderFakeUserAgent.Random}";
            var config = Configuration.Default.With(requester).WithDefaultLoader();
            context = BrowsingContext.New(config);
        }

        public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken cancellationToken)
        {
            if (novel == null || string.IsNullOrEmpty(novel.URL)) throw new ArgumentNullException(nameof(novel));

            var doc = await context.OpenAsync(novel.URL);

            var chapterCount = GetCountChapters(doc);

            if (chapterCount == novel?.ChaptersByGroup?.FirstOrDefault().Value.Count)
            {
                return novel;
            }

            var teams = new Dictionary<string, SortedList<int, Chapter>>() {
                { "none", await GetChapters(novel!.URL, chapterCount, cancellationToken) }
            };

            var coverUrl = GetCoverUrl(doc);
            var author = GetName(doc);

            novel.Author = author;
            novel.Name = $"{author}'s Kemono";
            novel.ChaptersByGroup = teams;
            //novel.Cover = await GetImg(coverUrl);

            return novel;
        }

        private async Task UpdateImages(Chapter chapter, bool includeImages)
        {
            var content = chapter.Content;

            if (content != null && (content.Contains("img") || content.Contains("fileThumb")))
            {
                var doc = parser.ParseDocument(content);

                foreach (var img in doc.QuerySelectorAll("img"))
                {
                    if (includeImages)
                    {
                        string? url = img.GetAttribute("src");

                        if (!string.IsNullOrEmpty(url))
                        {
                            var image = await GetImg(kemonoUrl + img.GetAttribute("src"));
                            var name = Guid.NewGuid().ToString();
                            //chapter.Images.Add(name, image);
                            img.SetAttribute("src", name);
                            img.RemoveAttribute("data-src");
                        }
                    }
                    else
                    {
                        img.Remove();
                    }
                }

                foreach (var a in doc.QuerySelectorAll(".fileThumb"))
                {
                    if (includeImages)
                    {
                        string? href = a.GetAttribute("href");

                        if (!string.IsNullOrEmpty(href))
                        {
                            if (href.StartsWith("/data"))
                            {
                                href = kemonoUrl + href;
                            }
                            a.SetAttribute("href", href);
                        }
                    }
                    else
                    {
                        a.Remove();
                    }
                }

                chapter.Content = doc.Body?.InnerHtml ?? "";
            }

            chapter.ImagesLoaded = includeImages;
        }

        private string GetName(IDocument doc) => doc.QuerySelector(".user-header__profile")?.TextContent.Trim() ?? "";

        private string GetCoverUrl(IDocument doc) => kemonoUrl + doc.QuerySelector(".image-link .fancy-image__image")?.GetAttribute("src");

        private async Task<byte[]> GetImg(string url)
        {
            byte[] result = new byte[0];

            var attempt = 1;

            while (attempt < 4)
            {
                try
                {
                    result = await httpClient.GetByteArrayAsync(url);
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

        private int GetCountChapters(IDocument doc)
        {
            var paginator = doc.QuerySelector(".paginator > small")?.TextContent ?? "of 0";
            return int.Parse(paginator.Substring(paginator.IndexOf("of") + 2).Trim());
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
                    var chapterUrl = kemonoUrl + item.GetAttribute("href");
                    var title = item.TextContent;
                    chapters.Add(j, new Chapter() { Name = title, Url = chapterUrl, Content = "", Number = j.ToString() });
                    j--;
                }

                setProgress(count, i);
                if (cancellationToken.IsCancellationRequested) return chapters;
            }

            return chapters;
        }

        public bool ValidateUrl(string url)
        {
            return !string.IsNullOrEmpty(PrepareUrl(url));
        }

        public string PrepareUrl(string url)
        {
            return Regex.Match(url, urlPattern).Value;
        }
    }
}