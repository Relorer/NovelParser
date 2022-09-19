using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Ranobelib
{
    public class RanobelibParser : INovelParser
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
                                                          (dict[br.name] = window.__DATA__.chapters.list
                                                            .filter((ch) => ch.branch_id === br.id || br.id === "nobranches")
                                                            .map((ch) => ({
                                                              Name: ch.chapter_name,
                                                              Url: `https://ranobelib.me/${window.__DATA__.manga.slug}/v${ch.chapter_volume}/c${ch.chapter_number}`,
                                                              Number: ch.chapter_number,
                                                              Index: ch.index,
                                                            }))
                                                            .reverse())
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
        private const string ranobelibUrl = "https://ranobelib.me/";

        private readonly HtmlParser parser = new HtmlParser();

        public async Task<Novel> ParseAsync(string ranobeUrl)
        {
            Novel novel;
            using (var driver = ChromeDriverHelper.StartChrome())
            {
                driver.GoTo(PrepareUrl(ranobeUrl));
                novel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(GetNovelInfoScript)) ?? new Novel();

                novel.ChaptersByTranslationTeam = JsonConvert.DeserializeObject<Dictionary<string, List<Chapter>>>((string)driver.ExecuteScript(GetChaptersScript));
            }

            novel.CoverPath = await DownloadCover(novel.CoverUrl, novel.NameEng);

            return novel;
        }

        public async Task ParseAndLoadChapters(Novel novel, string translationTeam)
        {
            List<Chapter>? chapters;
            if (novel.ChaptersByTranslationTeam != null && novel.ChaptersByTranslationTeam.TryGetValue(translationTeam, out chapters))
            {
                foreach (var item in chapters)
                {
                    if (string.IsNullOrEmpty(item.Content))
                    {
                        await ParseChapter(item);
                    }
                }
            }
        }

        private async Task ParseChapter(Chapter chapter)
        {
            // open a new сhrome driver for each chapter to avoid blocking
            using (var driver = ChromeDriverHelper.StartChrome())
            {
                driver.GoTo(chapter.Url);
                chapter.Content = (string?)driver.ExecuteScript("return document.querySelector('.reader-container')?.innerHTML");
            }

            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                var document = parser.ParseDocument(chapter.Content);

                foreach (var item in document.QuerySelectorAll("img"))
                {
                    string? url = item.GetAttribute("data-src");
                    if (!string.IsNullOrEmpty(url))
                    {
                        var base64 = GetImageAsBase64Url(url);
                        item.SetAttribute("src", await base64);
                    }
                }

                chapter.Content = document?.Body?.InnerHtml;
            }
        }

        private async Task<string> DownloadCover(string? url, string? novelName)
        {
            novelName = FileSystemHelper.RemoveInvalidFilePathCharacters(novelName ?? "", "-");
            novelName = $"cover-{novelName}.jpg";
            return !string.IsNullOrEmpty(url) && await DownloadImage(url, novelName) ? ChromeDriverHelper.GetDownloadedPath(novelName) : "";
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
            using var driver = ChromeDriverHelper.StartChrome();
            driver.GoTo(url);
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
            return url.StartsWith(ranobelibUrl) && (url.IndexOf('?') < 0 || url.IndexOf('?') > ranobelibUrl.Length);
        }

        private string PrepareUrl(string url)
        {
            return ranobelibUrl + Regex.Match(url.Substring(ranobelibUrl.Length), @"[^(?|\/)]*").Value;
        }
    }
}