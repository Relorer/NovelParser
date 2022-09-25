using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Ranobelib
{
    internal class RanobelibParser : INovelParser
    {
        private const string checkChallengeRunningScript = """
                                                    return JSON.stringify(document.querySelector("#challenge-running") !== null)
                                                  """;

        private const string downloadImgScript = """
                                                    var a = document.createElement("a");
                                                    a.href = "{0}";
                                                    a.download = "{1}";
                                                    a.click();
                                                  """;

        private const string getChaptersScript = """
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
                                                                  })
                                                              );
                                                            return chapter;
                                                          })())
                                                      );
                                                      return JSON.stringify(dict);
                                                    })();
                                                  """;

        private const string getCoverScript = """
                                                    return (
                                                      document.querySelector(`img[alt='${window.__DATA__.manga.name}']`) ||
                                                      document.querySelector("img.media-header__cover")
                                                    )?.src;
                                                  """;

        private const string getNovelInfoScript = """
                                                        return JSON.stringify({
                                                            Name:
                                                            window.__DATA__.manga.engName ||
                                                            window.__DATA__.manga.rusName ||
                                                            window.__DATA__.manga.slug,
                                                            Author:
                                                            [...document.querySelectorAll(".media-info-list__item")]
                                                                .find((item) => item.children[0].innerText === "Автор")
                                                                ?.children[1].textContent.trim() ?? "No Author",
                                                            Description: document
                                                            .querySelector(".media-description__text")
                                                            ?.textContent.trim(),
                                                        });
                                                  """;

        private const string ranobelibUrl = "https://ranobelib.me/";
        private readonly HtmlParser parser = new HtmlParser();

        private readonly SetProgress setProgress;

        public RanobelibParser(SetProgress setProgress)
        {
            this.setProgress = setProgress;
        }

        public Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var parsed = 1;
                var nonLoadedChapters = novel[group, pattern].Where(ch => string.IsNullOrEmpty(ch.Value.Content) || (ch.Value.ImagesLoaded ^ includeImages)).ToList();
                foreach (var item in nonLoadedChapters)
                {
                    await ParseChapter(item.Value, includeImages, cancellationToken);
                    setProgress(nonLoadedChapters.Count, parsed++);
                }
            });
        }

        public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                Novel? tempNovel;

                var coverUrl = "";

                using (var driver = await TryLoadPage(novel.URL!))
                {
                    tempNovel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(getNovelInfoScript));
                    coverUrl = (string)driver.ExecuteScript(getCoverScript);

                    if (tempNovel == null) return novel;

                    tempNovel.ChaptersByGroup = JsonConvert.DeserializeObject<Dictionary<string, SortedList<int, Chapter>>>((string)driver.ExecuteScript(getChaptersScript));
                }

                novel.Merge(tempNovel);

                if (novel.Cover != null || cancellationToken.IsCancellationRequested) return novel;

                novel.Cover = await GetImageAsByteArray(coverUrl);

                return novel;
            });
        }

        public string PrepareUrl(string url)
        {
            return ranobelibUrl + Regex.Match(url.Substring(ranobelibUrl.Length), @"[^(?|\/)]*").Value;
        }

        public bool ValidateUrl(string url)
        {
            return url.Length > ranobelibUrl.Length && url.StartsWith(ranobelibUrl) && PrepareUrl(url).Length > ranobelibUrl.Length;
        }

        private bool CheckChallengeRunning(ChromeDriver driver)
        {
            return JsonConvert.DeserializeObject<bool>((string)driver.ExecuteScript(checkChallengeRunningScript));
        }

        private async Task<byte[]> GetImageAsByteArray(string url)
        {
            byte[] result = new byte[0];

            string fileName = Guid.NewGuid().ToString() + ".tmp";
            string fullPath = ChromeDriverHelper.GetDownloadedPath(fileName);

            if (File.Exists(fullPath)) File.Delete(fullPath);

            using var driver = await TryLoadPage(url);

            driver.ExecuteScript(string.Format(downloadImgScript, url, fileName));

            await Task.Delay(200);

            var attempt = 1;
            while (!File.Exists(fullPath) || attempt < 4)
            {
                await Task.Delay(200 * attempt++);
            }

            if (File.Exists(fullPath))
            {
                result = File.ReadAllBytes(fullPath);
                File.Delete(fullPath);
            }

            return result;
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
    }
}