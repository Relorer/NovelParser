/*
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Parsers.Libme
{
    internal abstract class ComicsLibMeParser : BaseLibMeParser
    {
        private static readonly string getComicsContentScript = Resources.GetComicsContentScript;

        public ComicsLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        protected virtual List<string> servers => new List<string>()
        {
            "https://img3.cdnlib.link/",
            "https://img2.mixlib.me/",
            "https://img4.imgslib.link/",
        };

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
                    await ParseChapter(novel, item);
                    setProgress(nonLoadedChapters.Count, parsed++, Resources.ProgressStatusParsing);
                }

                if (includeImages)
                {
                    var allImages = novel[group, pattern].SelectMany(ch => ch.Value.Images).ToList();
                    await DownloadImages(allImages, novel.DownloadFolderName, cancellationToken);
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

        private async Task DownloadImages(List<ImageInfo> images, string downloadFolderName, CancellationToken cancellationToken)
        {
            if (!images.Any()) return;

            var notLoadedImages = images.Where(img => !img.Exists).ToList();

            var serversWithRate = servers.Select(s => new ServerWithRate(s)).ToList();

            var batchSize = 10;
            setProgress(notLoadedImages.Count, 0, Resources.ProgressStatusImageLoading);
            for (int i = 0; i < notLoadedImages.Count;)
            {
                var batch = notLoadedImages.GetRange(i, Math.Min(batchSize, notLoadedImages.Count - i)).Where(img => !img.Exists);

                foreach (var server in serversWithRate)
                {
                    var notLoadedImagesURLs = batch.Select(img => $"{server.Server + img.URL}?name={img.Name}").ToArray();

                    using (var driver = await ChromeDriverHelper.TryLoadPage(server.Server, checkChallengeRunningScript, downloadFolderName))
                    {
                        driver.ExecuteScript(downloadImagesScript, notLoadedImagesURLs);
                    }

                    await Task.Delay(2000);

                    var batchCount = batch.Count();
                    server.CountImages += notLoadedImagesURLs.Count() - batchCount;
                    if (batchCount == 0) break;
                }

                if (cancellationToken.IsCancellationRequested) return;

                i += batchSize;

                setProgress(notLoadedImages.Count, i, Resources.ProgressStatusImageLoading);
                batchSize = Math.Min(50, batchSize + 10);
                serversWithRate.Sort((s1, s2) => s2.CountImages - s1.CountImages);
            }
        }

        private async Task ParseChapter(Novel novel, Chapter chapter)
        {
            using (var driver = await ChromeDriverHelper.TryLoadPage(chapter.Url!, checkChallengeRunningScript))
            {
                chapter.Content = (string?)driver.ExecuteScript(getComicsContentScript);
            }

            if (chapter.Content != null && chapter.Content.Contains("img"))
            {
                (chapter.Content, chapter.Images) = await htmlHelper.LoadImagesForHTML(chapter.Content, (img) =>
                {
                    string url = img.GetAttribute("src") ?? "";
                    var imageInfo = new ImageInfo(novel.DownloadFolderName, url);
                    img.SetAttribute("src", imageInfo.Name);
                    return Task.FromResult(imageInfo);
                });
            }

            chapter.ImagesLoaded = true;
        }

        private class ServerWithRate
        {
            public ServerWithRate(string server)
            {
                Server = server;
            }

            public int CountImages { get; set; }
            public string Server { get; }
        }
    }
}*/