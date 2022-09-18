using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Ranobelib
{
    internal class RanobelibParser : INovelParser
    {
        private const string urlPattern = @"https:\/\/ranobelib.me\/([\s\S]+?)[\/?]";
        private const string downloadedFileName = "RanobelibParserImg.jpg";

        private const string GetNovelInfoScript = """
                                                    return JSON.stringify({
                                                        NameRus: window.__DATA__.manga.rusName,
                                                        NameEng: window.__DATA__.manga.engName,
                                                        CoverUrl: (document.querySelector(`img[alt='${window.__DATA__.manga.name}']`) || document.querySelector("img.media-header__cover"))?.src,
                                                        Author: document
                                                            .querySelectorAll(".media-info-list__item")[document.querySelectorAll(".media-info-list__item").length === 9 ? 5 : 4]
                                                            ?.children[1].textContent.trim(),
                                                        Description: document
                                                            .querySelector(".media-description__text")
                                                            ?.textContent.trim(),
                                                    });
                                                  """;

        private const string GetChaptersScript = """
                                                    return JSON.stringify(
                                                        (window.__DATA__.chapters.branches.length > 0
                                                        ? window.__DATA__.chapters.branches
                                                        : [{ id: "nobranches" }]
                                                        )
                                                        .map((br) => br.id)
                                                        .map((id) =>
                                                            window.__DATA__.chapters.list
                                                            .filter((ch) => ch.branch_id === id || id === "nobranches")
                                                            .map((ch) => ({
                                                                Name: ch.chapter_name,
                                                                Url: `https://ranobelib.me/${window.__DATA__.manga.slug}/v${ch.chapter_volume}/c${ch.chapter_number}`,
                                                                Number: ch.chapter_number,
                                                                Index: ch.index,
                                                            }))
                                                        )
                                                    );
                                                  """;

        private const string DownloadImgScript = """
                                                    var a = document.createElement("a");
                                                    a.href = "{0}";
                                                    a.download = "{1}";
                                                    a.click();
                                                  """;

        private readonly HtmlParser parser = new HtmlParser();

        public async Task<Novel> ParseAsync(string url)
        {
            Novel novel;
            using (var driver = ChromeDriverHelper.StartChrome())
            {
                driver.GoTo(PrepareUrl(url));
                novel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(GetNovelInfoScript)) ?? new Novel();

                List<List<Chapter>>? data = JsonConvert.DeserializeObject<List<List<Chapter>>>((string)driver.ExecuteScript(GetChaptersScript));

                //TODO add selector
                novel.Chapters = data?.MaxBy(d => d.Count) ?? new List<Chapter>();
                novel.Chapters.Reverse();
            }

            novel.CoverPath = await DownloadCover(novel.CoverUrl, novel.NameEng);

            return novel;
        }

        public async Task ParseAndLoadChapters(Novel novel)
        {
            if (novel.Chapters != null)
            {
                foreach (var item in novel.Chapters)
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
            return Regex.Match(url, urlPattern).Success;
        }

        private string PrepareUrl(string url)
        {
            return Regex.Match(url, urlPattern).Value;
        }
    }
}
