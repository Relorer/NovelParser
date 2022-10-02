using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using NovelParserBLL.Utilities;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.libme
{
    internal abstract class ComixLibMeParser : BaseLibMeParser
    {
        private static readonly string getComixContentScript = Resources.GetComixContentScript;

        protected virtual List<string> servers => new List<string>()
        {
            "https://img3.cdnlib.link/",
            "https://img2.mixlib.me/",
            "https://img4.imgslib.link/",
        };

        protected abstract string domen { get; }

        public ComixLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var parsed = 1;
                var nonLoadedChapters = novel[group, pattern].Where(ch => string.IsNullOrEmpty(ch.Value.Content) || !ch.Value.ImagesLoaded && includeImages).ToList();
                foreach (var item in nonLoadedChapters)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    await ParseChapter(novel, item.Value);
                    setProgress(nonLoadedChapters.Count, parsed++);
                }

                if (includeImages)
                {
                    var allImages = novel[group, pattern].SelectMany(ch => ch.Value.Images).ToList();
                    await DownloadImages(allImages);
                }
            });
        }

        public override string PrepareUrl(string url)
        {
            return domen + Regex.Match(url.Substring(domen.Length), @"[^(?|\/)]*").Value;
        }

        public override bool ValidateUrl(string url)
        {
            return url.Length > domen.Length && url.StartsWith(domen) && PrepareUrl(url).Length > domen.Length;
        }

        private async Task ParseChapter(Novel novel, Chapter chapter)
        {
            var downloadFolderName = FileHelper.RemoveInvalidFilePathCharacters(novel.URL!);
            var downloadPath = Path.Combine(ChromeDriverHelper.DownloadPath, downloadFolderName);

            using (var driver = await ChromeDriverHelper.TryLoadPage(chapter.Url!, checkChallengeRunningScript, downloadFolderName))
            {
                chapter.Content = (string?)driver.ExecuteScript(getComixContentScript);
            }

            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                (chapter.Content, chapter.Images) = await htmlHelper.LoadImagesForHTML(chapter.Content, (img) =>
                {
                    string url = img.GetAttribute("src") ?? "";
                    var imageInfo = new ImageInfo(downloadPath, url);
                    img.SetAttribute("src", imageInfo.Name);
                    return Task.FromResult(imageInfo);
                });
            }
        }

        private async Task DownloadImages(List<ImageInfo> images)
        {
            if (!images.Any()) return;
            var firstImg = images.First();
            var downloadFolderName = Path.GetDirectoryName(firstImg.FullPath)!;

            var notLoadedImages = images.Where(img => !img.Exists);

            foreach (var server in servers)
            {
                var notLoadedImagesURLs = notLoadedImages.Select(img => server + img.URL).ToArray();
                var fileNames = notLoadedImages.Select(img => new
                {
                    Before = Path.Combine(downloadFolderName, img.NameFromURL),
                    After = Path.Combine(downloadFolderName, img.Name),
                }
                ).ToArray();

                using (var driver = await ChromeDriverHelper.TryLoadPage(server, checkChallengeRunningScript, downloadFolderName))
                {
                    driver.ExecuteScript(downloadImagesScript, notLoadedImagesURLs);
                }

                await Task.Delay(2000);

                foreach (var item in fileNames)
                {
                    if (File.Exists(item.Before))
                    {
                        File.Move(item.Before, item.After);
                    }
                }

                if (images.Exists()) break;
            }
        }
    }
}