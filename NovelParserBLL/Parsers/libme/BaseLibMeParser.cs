using Newtonsoft.Json;
using NovelParserBLL.Models;
using NovelParserBLL.Properties;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Parsers.libme
{
    internal abstract class BaseLibMeParser : INovelParser
    {
        protected static readonly string checkChallengeRunningScript = Resources.CheckChallengeRunningScript;
        protected static readonly string downloadImagesScript = Resources.DownloadImagesScript;
        protected readonly HTMLHelper htmlHelper = new HTMLHelper();
        protected readonly SetProgress setProgress;
        private static readonly string getChaptersScript = Resources.GetChaptersScript;
        private static readonly string getNovelInfoScript = Resources.GetNovelInfoScript;

        public abstract string SiteDomen { get; }

        public abstract string SiteName { get; }

        public ParserInfo ParserInfo => new ParserInfo(SiteDomen, SiteName, "https://lib.social/login");

        public BaseLibMeParser(SetProgress setProgress)
        {
            this.setProgress = setProgress;
        }

        public abstract Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken cancellationToken);

        public async Task<Novel> ParseCommonInfo(Novel novel, CancellationToken _)
        {
            return await Task.Run(async () =>
            {
                Novel? tempNovel;

                setProgress(0, 0, Resources.ProgressStatusLoading);

                using (var driver = await ChromeDriverHelper.TryLoadPage(novel.URL!, checkChallengeRunningScript, novel.DownloadFolderName))
                {
                    tempNovel = JsonConvert.DeserializeObject<Novel>((string)driver.ExecuteScript(getNovelInfoScript, !(novel.Cover?.Exists ?? false)));

                    if (tempNovel == null) return novel;

                    tempNovel.ChaptersByGroup = JsonConvert.DeserializeObject<Dictionary<string, SortedList<int, Chapter>>>((string)driver.ExecuteScript(getChaptersScript));
                    novel.Merge(tempNovel);

                    novel.Cover = FileHelper.UpdateImageInfo(novel.Cover!, novel.DownloadFolderName);
                }

                return novel;
            });
        }

        public abstract string PrepareUrl(string url);

        public abstract bool ValidateUrl(string url);
    }
}