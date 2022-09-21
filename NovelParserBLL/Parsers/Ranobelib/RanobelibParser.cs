using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Ranobelib
{
    public class RanobelibParser
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
                Novel? novel;

                using (var driver = await TryLoadPage(PrepareUrl(ranobeUrl)))
                {
                    novel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(GetNovelInfoScript));

                    if (novel == null) return null;

                    novel.ChaptersByTranslationTeam = JsonConvert.DeserializeObject<Dictionary<string, SortedList<int, Chapter>>>((string)driver.ExecuteScript(GetChaptersScript));
                }

                novel.Cover = await DownloadCover(novel.CoverUrl, novel.NameEng);
                return novel;
            });
        }

        public Task ParseAndLoadChapters(SortedList<int, Chapter> chapters, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                foreach (var item in chapters)
                {
                    if (string.IsNullOrEmpty(item.Value.Content) || (item.Value.ImagesLoaded ^ includeImages))
                    {
                        await ParseChapter(item.Value, includeImages);
                    }
                }
            });
        }

        private async Task ParseChapter(Chapter chapter, bool includeImages = true)
        {
            // open a new сhrome driver for each chapter to avoid blocking
            using (var driver = await TryLoadPage(chapter.Url))
            {
                chapter.Content = (string)driver.ExecuteScript("return document.querySelector('.reader-container')?.innerHTML");
            }

            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                var document = parser.ParseDocument(chapter.Content);

                foreach (var item in document.QuerySelectorAll("img"))
                {
                    if (includeImages)
                    {
                        string? url = item.GetAttribute("data-src");
                        if (!string.IsNullOrEmpty(url))
                        {
                            var base64 = GetImageAsBase64Url(url);
                            item.SetAttribute("src", await base64);
                        }
                    }
                    else
                    {
                        item.Remove();
                    }
                }

                chapter.Content = document.Body?.InnerHtml ?? "";
            }

            chapter.ImagesLoaded = includeImages;
        }

        private async Task<byte[]?> DownloadCover(string? url, string? novelName)
        {
            novelName = FileSystemHelper.RemoveInvalidFilePathCharacters(novelName ?? "", "-");
            novelName = $"cover-{novelName}.jpg";
            var path = !string.IsNullOrEmpty(url) &&
                await DownloadImage(url, novelName) ? ChromeDriverHelper.GetDownloadedPath(novelName) : "";

            byte[]? cover = null;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                cover = File.ReadAllBytes(path);
                File.Delete(path);
            }
            return cover;
        }

        private async Task<string> GetImageAsBase64Url(string url)
        {
            string result = "";

            string fullPath = ChromeDriverHelper.GetDownloadedPath(downloadedFileName);

            if (await DownloadImage(url, downloadedFileName))
            {
                result = ImgHelper.ImageFileToBase64(fullPath);
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

        public ChromeDriver OpenAuthPage()
        {
            var driver = ChromeDriverHelper.StartChrome(true);
            driver.GoTo("https://lib.social/login");

            Task.Run(async () =>
            {
                await Task.Delay(15 * 60_000);
                driver?.Close();
                driver?.Dispose();
            });

            return driver;
        }
    }
}