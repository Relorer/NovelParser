using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Kemono
{
    internal class KemonoParser : INovelParser
    {
        private readonly string kemonoUrl = "https://kemono.party/";
        private readonly string urlPattern = @"https:\/\/kemono\.party\/patreon\/user\/\d*";

        private readonly HtmlParser parser = new HtmlParser();
        private readonly HttpClient httpClient = new HttpClient();

        private readonly SetProgress setProgress;

        public KemonoParser(SetProgress setProgress)
        {
            this.setProgress = setProgress;
        }

        private IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

        public Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                int i = 1;
                var chapters = novel[group, pattern];
                foreach (var chapter in chapters.Values)
                {
                    var doc = await context.OpenAsync(chapter.Url!);
                    chapter.Content = doc.QuerySelector(".post__body")!.InnerHtml;

                    if (includeImages) await LoadImage(chapter);

                    setProgress(chapters.Count, i);

                    if (cancellationToken.IsCancellationRequested) return;
                }
            });
        }

        public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken cancellationToken)
        {
            var doc = await context.OpenAsync(novel.URL!);

            var chapterCount = GetCountChapters(doc);

            var teams = new Dictionary<string, SortedList<int, Chapter>>() {
                { "none", await GetChapters(novel.URL!, chapterCount, cancellationToken) }
            };

            var coverUrl = GetCoverUrl(doc);
            var author = GetName(doc);

            novel.Author = author;
            novel.Name = "No name";
            novel.ChaptersByGroup = teams;
            novel.Cover = await GetImg(coverUrl);

            return novel;
        }

        private async Task LoadImage(Chapter chapter)
        {
            var content = chapter.Content;

            if (content != null && content.Contains("img"))
            {
                var doc = parser.ParseDocument(content);

                foreach (var img in doc.QuerySelectorAll("img"))
                {
                    string? url = img.GetAttribute("src");

                    if (!string.IsNullOrEmpty(url))
                    {
                        var image = await GetImg(kemonoUrl + img.GetAttribute("src"));
                        var name = Guid.NewGuid().ToString();
                        chapter.Images.Add(name, image);
                        img.SetAttribute("src", name);
                    }
                }

                chapter.Content = doc.Body?.InnerHtml ?? "";
            }
        }

        private string GetName(IDocument doc) => doc.QuerySelector(".user-header__profile")?.TextContent.Trim() ?? "";

        private string GetCoverUrl(IDocument doc) => kemonoUrl + doc.QuerySelector(".fancy-image__image")?.GetAttribute("src");

        private async Task<byte[]> GetImg(string url) => await httpClient.GetByteArrayAsync(url);

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
