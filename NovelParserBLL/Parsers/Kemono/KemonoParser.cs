using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NovelParserBLL.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Kemono
{
    internal class KemonoParser : INovelParser
    {
        private readonly string kemonoUrl = "https://kemono.party/";
        private readonly string urlPattern = @"https:\/\/kemono\.party\/patreon\/user\/\d*";

        private readonly HtmlParser parser = new HtmlParser();

        private IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

        public Task ParseAndLoadChapters(Novel novel, SortedList<int, Chapter> chapters, bool includeImages, Action<int, int> setProgress, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                foreach (var item in chapters)
                {
                    var chapter = item.Value;
                    var page = await context.OpenAsync(chapter.Url);

                    chapter.Content = page.QuerySelector(".post__body")!.InnerHtml;

                    if (chapter.Content != null && chapter.Content.Contains("img"))
                    {
                        var document = parser.ParseDocument(chapter.Content);

                        foreach (var img in document.QuerySelectorAll("img"))
                        {
                            if (includeImages)
                            {
                                string? url = img.GetAttribute("data-src") ?? img.GetAttribute("src");

                                if (!string.IsNullOrEmpty(url))
                                {
                                    using var webClient = new HttpClient();
                                    var image = await webClient.GetByteArrayAsync(kemonoUrl + img.GetAttribute("src"));
                                    var name = Guid.NewGuid().ToString();
                                    chapter.Images.Add(name, image);
                                    img.SetAttribute("src", name);
                                }
                            }
                            else
                            {
                                img.Remove();
                                chapter.Images.Clear();
                            }
                        }

                        chapter.Content = document.Body?.InnerHtml ?? "";
                    }

                    
                    Console.WriteLine(chapter.Index);
                }
            });
        }

        public async Task<Novel?> ParseAsync(string novelUrl, CancellationToken cancellationToken)
        {
            novelUrl = PrepareUrl(novelUrl);

            var document = await context.OpenAsync(novelUrl);

            var name = kemonoUrl + document.QuerySelector(".user-header__profile")?.TextContent.Trim();
            var coverUrl = kemonoUrl + document.QuerySelector(".fancy-image__image")?.GetAttribute("src");
            var paginator = document.QuerySelector(".paginator > small")?.TextContent ?? "of 0";

            var countPosts = int.Parse(paginator.Substring(paginator.IndexOf("of") + 2).Trim());

            var teams = new Dictionary<string, SortedList<int, Chapter>>() { { "none", new SortedList<int, Chapter>()} };
            var chapters = teams.First().Value;

            var index = countPosts;

            for (int i = 0; i < countPosts; i += 25)
            {
                var page = await context.OpenAsync(novelUrl + $"?o={i}");

                foreach (var item in page.QuerySelectorAll(".post-card__heading > a"))
                {
                    var url = kemonoUrl + item.GetAttribute("href");
                    var title = item.TextContent;
                    chapters.Add(index, new Chapter(title, url, "", index.ToString(), index));
                    index--;
                }
            }
            

            var novel = new Novel(name, name, coverUrl, name, "");
            novel.ChaptersByTranslationTeam = teams;
            novel.URL = novelUrl;

            return novel;
        }

        public bool ValidateUrl(string url)
        {
            return !string.IsNullOrEmpty(PrepareUrl(url));
        }

        private string PrepareUrl(string url)
        {
            return Regex.Match(url, urlPattern).Value;
        }
    }
}
