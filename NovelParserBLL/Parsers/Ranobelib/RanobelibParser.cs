using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Ranobelib
{
    internal class RanobelibParser : INovelParser
    {
        private const string downloadedFileName = "RanobelibParserImg.jpg";

        private const string GetNovelInfoScript = """
                                                        return JSON.stringify({
                                                            NameRus:
                                                            window.__DATA__.manga.rusName ||
                                                            window.__DATA__.manga.engName ||
                                                            window.__DATA__.manga.slug,
                                                            NameEng:
                                                            window.__DATA__.manga.engName ||
                                                            window.__DATA__.manga.rusName ||
                                                            window.__DATA__.manga.slug,
                                                            CoverUrl: (
                                                            document.querySelector(`img[alt='${window.__DATA__.manga.name}']`) ||
                                                            document.querySelector("img.media-header__cover")
                                                            )?.src,
                                                            Author:
                                                            [...document.querySelectorAll(".media-info-list__item")]
                                                                .find((item) => item.children[0].innerText === "Автор")
                                                                ?.children[1].textContent.trim() ?? "No Author",
                                                            Description: document
                                                            .querySelector(".media-description__text")
                                                            ?.textContent.trim(),
                                                        });
                                                  """;

        private const string GetChaptersScript = """
                                                    return (() => {
                                                      const dict = {};
                                                      (window.__DATA__.chapters.branches.length > 0
                                                        ? window.__DATA__.chapters.branches
                                                        : [{ id: "nobranches", name: "none" }]
                                                      ).forEach(
                                                        (br) =>
                                                          (dict[br.name] = (() => {
                                                            const chapter = {};
                                                            window.__DATA__.chapters.list
                                                              .filter((ch) => ch.branch_id === br.id || br.id === "nobranches")
                                                              .forEach(
                                                                (ch) =>
                                                                  (chapter[ch.index] = {
                                                                    Name: ch.chapter_name,
                                                                    Url: `https://ranobelib.me/${window.__DATA__.manga.slug}/v${ch.chapter_volume}/c${ch.chapter_number}`,
                                                                    Number: ch.chapter_number,
                                                                    Index: ch.index,
                                                                  })
                                                              );
                                                            return chapter;
                                                          })())
                                                      );
                                                      return JSON.stringify(dict);
                                                    })();
                                                  """;

        private const string DownloadImgScript = """
                                                    var a = document.createElement("a");
                                                    a.href = "{0}";
                                                    a.download = "{1}";
                                                    a.click();
                                                  """;

        private const string CheckChallengeRunningScript = """
                                                    return JSON.stringify(document.querySelector("#challenge-running") !== null)
                                                  """;

        private const string ranobelibUrl = "https://ranobelib.me/";

        private readonly HtmlParser parser = new HtmlParser();

        public async Task<Novel?> ParseAsync(string ranobeUrl, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                var url = PrepareUrl(ranobeUrl);
                Novel? novel;

                Novel? novelFromCache = NovelCacheService.TryGetNovelFromFile(url);

                if (cancellationToken.IsCancellationRequested) return null;

                using (var driver = await TryLoadPage(url))
                {
                    novel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(GetNovelInfoScript));

                    if (novel == null) return novelFromCache;

                    novel.ChaptersByTranslationTeam = JsonConvert.DeserializeObject<Dictionary<string, SortedList<int, Chapter>>>((string)driver.ExecuteScript(GetChaptersScript));
                }

                if (novelFromCache != null)
                {
                    novel.Cover = novelFromCache.Cover;
                    if (novel.ChaptersByTranslationTeam != null)
                    {
                        foreach (var team in novel.ChaptersByTranslationTeam)
                        {
                            if (
                                    novelFromCache.ChaptersByTranslationTeam != null &&
                                    novelFromCache.ChaptersByTranslationTeam.TryGetValue(team.Key, out SortedList<int, Chapter>? chapters)
                                )
                            {
                                foreach (var item in team.Value)
                                {
                                    if (chapters.TryGetValue(item.Key, out Chapter? chapter) && !string.IsNullOrEmpty(chapter.Content))
                                    {
                                        item.Value.Content = chapter.Content;
                                        item.Value.Images = chapter.Images;
                                        item.Value.ImagesLoaded = chapter.ImagesLoaded;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (cancellationToken.IsCancellationRequested) return null;
                    novel.Cover = await GetImageAsByteArray(novel.CoverUrl);
                }

                novel.URL = url;
                NovelCacheService.SaveNovelToFile(novel);
                return novel;
            });
        }

        public Task ParseAndLoadChapters(Novel novel, SortedList<int, Chapter> chapters, bool includeImages, Action<int, int> setProgress, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var parsed = 1;
                try
                {
                    foreach (var item in chapters)
                    {
                        if (string.IsNullOrEmpty(item.Value.Content) || (item.Value.ImagesLoaded ^ includeImages))
                        {
                            await ParseChapter(item.Value, includeImages, cancellationToken);
                            setProgress(chapters.Count, parsed++);
                        }
                    }
                }
                finally
                {
                    NovelCacheService.SaveNovelToFile(novel);
                }
            });
        }

        private async Task ParseChapter(Chapter chapter, bool includeImages, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            // open a new сhrome driver for each chapter to avoid blocking
            using (var driver = await TryLoadPage(chapter.Url))
            {
                chapter.Content = (string)driver.ExecuteScript("return document.querySelector('.reader-container')?.innerHTML");
            }

            if (cancellationToken.IsCancellationRequested) return;

            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                var document = parser.ParseDocument(chapter.Content);

                foreach (var item in document.QuerySelectorAll("img"))
                {
                    if (includeImages)
                    {
                        string? url = item.GetAttribute("data-src") ?? item.GetAttribute("src");
                        if (url != null && !url.Contains(ranobelibUrl)) url = ranobelibUrl + url;
                        if (!string.IsNullOrEmpty(url))
                        {
                            var image = GetImageAsByteArray(url);
                            var name = Guid.NewGuid().ToString();
                            chapter.Images.Add(name, await image);
                            item.SetAttribute("src", name);
                        }
                    }
                    else
                    {
                        item.Remove();
                        chapter.Images.Clear();
                    }
                }

                chapter.Content = document.Body?.InnerHtml ?? "";
            }

            chapter.ImagesLoaded = includeImages;
        }

        private async Task<byte[]> GetImageAsByteArray(string url)
        {
            byte[] result = new byte[0];

            string fullPath = ChromeDriverHelper.GetDownloadedPath(downloadedFileName);

            if (await DownloadImage(url, downloadedFileName))
            {
                result = File.ReadAllBytes(fullPath);
                File.Delete(fullPath);
            }

            return result;
        }

        private async Task<bool> DownloadImage(string url, string name)
        {
            var fullPath = ChromeDriverHelper.GetDownloadedPath(name);
            if (File.Exists(fullPath)) File.Delete(fullPath);

            using var driver = await TryLoadPage(url);

            driver.ExecuteScript(string.Format(DownloadImgScript, url, name));
            var attempt = 1;
            while (!File.Exists(fullPath) || attempt < 4)
            {
                await Task.Delay(200 * attempt++);
            }
            return File.Exists(ChromeDriverHelper.GetDownloadedPath(name));
        }

        public bool ValidateUrl(string url)
        {
            return url.Length > ranobelibUrl.Length && url.StartsWith(ranobelibUrl) && PrepareUrl(url).Length > ranobelibUrl.Length;
        }

        private string PrepareUrl(string url)
        {
            return ranobelibUrl + Regex.Match(url.Substring(ranobelibUrl.Length), @"[^(?|\/)]*").Value;
        }

        private async Task<ChromeDriver> TryLoadPage(string url)
        {
            var driver = ChromeDriverHelper.StartChrome();
            driver.GoTo(url);
            var attempt = 1;

            while (CheckChallengeRunning(driver))
            {
                driver.Dispose();

                driver = ChromeDriverHelper.StartChrome();

                driver.GoTo(url);

                await Task.Delay(attempt++ * 2000);
            }

            await Task.Delay(attempt * 1000);

            return driver;
        }

        private bool CheckChallengeRunning(ChromeDriver driver)
        {
            return JsonConvert.DeserializeObject<bool>((string)driver.ExecuteScript(CheckChallengeRunningScript));
        }

    }
}