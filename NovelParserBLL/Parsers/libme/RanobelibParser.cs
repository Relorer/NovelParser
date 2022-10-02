using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using NovelParserBLL.Utilities;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.libme
{
    internal class RanobelibParser : BaseLibMeParser
    {
        private const string ranobelibUrl = "https://ranobelib.me/";
        private static readonly string getRanobeContentScript = Resources.GetRanobeContentScript;

        public RanobelibParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var parsed = 1;
                var nonLoadedChapters = novel[group, pattern].Select(v => v.Value).Where(ch => string.IsNullOrEmpty(ch.Content) || (!ch.ImagesLoaded || !ch.Images.Exists()) && includeImages).ToList();
                foreach (var item in nonLoadedChapters)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    await ParseChapter(novel, item, includeImages);
                    setProgress(nonLoadedChapters.Count, parsed++);
                }
            });
        }

        public override string PrepareUrl(string url)
        {
            return ranobelibUrl + Regex.Match(url.Substring(ranobelibUrl.Length), @"[^(?|\/)]*").Value;
        }

        public override bool ValidateUrl(string url)
        {
            return url.Length > ranobelibUrl.Length && url.StartsWith(ranobelibUrl) && PrepareUrl(url).Length > ranobelibUrl.Length;
        }

        private async Task ParseChapter(Novel novel, Chapter chapter, bool includeImages)
        {
            using (var driver = await ChromeDriverHelper.TryLoadPage(chapter.Url!, checkChallengeRunningScript, novel.DownloadFolderName))
            {
                chapter.Content = (string?)driver.ExecuteScript(getRanobeContentScript, includeImages) ?? "";
            }

            if (includeImages)
            {
                await Task.Delay(2000);
                (chapter.Content, chapter.Images) = await htmlHelper.LoadImagesForHTML(chapter.Content, (img) =>
                {
                    string url = img.GetAttribute("src") ?? "";
                    return Task.FromResult(FileHelper.UpdateImageInfo(new ImageInfo("", "", url), novel));
                });
            }

            chapter.ImagesLoaded = includeImages;
        }
    }
}