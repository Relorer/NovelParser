using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using NovelParserBLL.Utilities;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.libme
{
    internal class RanobeLibMeParser : BaseLibMeParser
    {
        private static readonly string getRanobeContentScript = Resources.GetRanobeContentScript;

        public RanobeLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override string SiteDomen => "https://ranobelib.me/";

        public override string SiteName => "RanobeLib.me";

        public override Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var parsed = 1;
                var nonLoadedChapters = novel[group, pattern].ForLoad(includeImages);
                setProgress(nonLoadedChapters.Count, 0, Resources.ProgressStatusParsing);
                foreach (var item in nonLoadedChapters)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    await ParseChapter(novel, item, includeImages);
                    setProgress(nonLoadedChapters.Count, parsed++, Resources.ProgressStatusParsing);
                }
            });
        }

        public override string PrepareUrl(string url)
        {
            return SiteDomen + Regex.Match(url.Substring(SiteDomen.Length), @"[^(?|\/)]*").Value;
        }

        public override bool ValidateUrl(string url)
        {
            return url.Length > SiteDomen.Length && url.StartsWith(SiteDomen) && PrepareUrl(url).Length > SiteDomen.Length;
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
                    return Task.FromResult(FileHelper.UpdateImageInfo(new ImageInfo("", "", url), novel.DownloadFolderName));
                });
            }

            chapter.ImagesLoaded = includeImages;
        }
    }
}