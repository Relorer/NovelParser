using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
using NovelParserWPF.DialogWindows;
using NovelParserWPF.Utilities;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NovelParserWPF.ViewModels
{
    [GenerateViewModel]
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NovelCacheService novelCacheService;
        private readonly CommonNovelParser commonNovelParser;
        private readonly FileGeneratorService fileGeneratorService;

        public MainWindowViewModel()
        {
            novelCacheService = new NovelCacheService();
            commonNovelParser = new CommonNovelParser(novelCacheService, SetProgressValueProgressButton);
            fileGeneratorService = new FileGeneratorService();

            ParserURLs = commonNovelParser.ParserURLs;

            FileFormatsForGenerator = new List<RadioButton>() {
                new RadioButton() {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormat.EPUB,
                    IsChecked = true
                },
                new RadioButton() {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormat.PDF,
                }
            };
        }

        public List<RadioButton> FileFormatsForGenerator { get; set; }

        public Novel? Novel { get; set; }

        public List<string> TranslationTeams => Novel?.ChaptersByGroup?.Keys.ToList() ?? new List<string>();

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;

        private SortedList<int, Chapter>? chaptersCurrentTeam => Novel?[SelectedTranslationTeam, "all"];

        public SortedList<int, Chapter>? ChaptersToDownload => Novel?[SelectedTranslationTeam, ListChaptersPattern];

        public BitmapImage? Cover => Novel?.Cover?.TryGetByteArray(out byte[]? cover) ?? false ? ImageHelper.BitmapImageFromBuffer(cover!) : null;

        #region ParsingParams

        private string novelLink = "";

        public string NovelLink
        {
            get => novelLink;
            set
            {
                novelLink = value;
                Novel = null;
                ProgressButtonText = "Get";
            }
        }

        public string SelectedTranslationTeam { get; set; } = "";

        public string ListChaptersPattern { get; set; } = "All";

        public bool IncludeImages { get; set; } = true;

        public string SavePath { get; set; } = "";

        public string FileName => Path.GetFileName(SavePath);

        [GenerateCommand]
        private void ResetNovelLink()
        {
            NovelLink = "";
        }

        [GenerateCommand]
        private void SelectSavePath()
        {
            SavePath = FileDialogHelper.GetSaveFilePath(SavePath, GetSelectedFileFormat());
        }

        private bool CanSelectSavePath() => Novel != null;

        #endregion ParsingParams

        #region Parsing

        public string ProgressButtonText { get; set; } = "Get";
        public int ProgressValueProgressButton { get; set; } = 0;
        public bool NovelLoadingLock { get; set; } = false;

        private CancellationTokenSource? cancellationTokenSource = null;

        private Task? loadingTask;

        private bool isLoadingProgressButton = false;

        public bool IsLoadingProgressButton
        {
            get => isLoadingProgressButton;
            set
            {
                if (isLoadingProgressButton && !value)
                {
                    cancellationTokenSource?.Cancel();
                    ProgressButtonText = "Canceling";
                    if (loadingTask == null || loadingTask.Status > TaskStatus.WaitingForChildrenToComplete)
                    {
                        cancellationTokenSource?.Dispose();
                        isLoadingProgressButton = false;
                        ProgressButtonText = Novel == null ? "Get" : "Start";
                    }
                }
                else if (value)
                {
                    isLoadingProgressButton = true;
                    cancellationTokenSource = new CancellationTokenSource();
                    loadingTask = StartButtonClick(cancellationTokenSource.Token);
                    loadingTask.ContinueWith((_) =>
                    {
                        IsLoadingProgressButton = false;
                    });
                    ProgressButtonText = Novel == null ? "Loading" : "Parsing";
                }
            }
        }

        private async Task StartButtonClick(CancellationToken cancellationToken)
        {
            if (!commonNovelParser.ValidateUrl(NovelLink)) return;

            try
            {
                TryCloseAuthDriver();

                if (Novel == null)
                {
                    await GetNovelInfo(cancellationToken);
                }
                else
                {
                    await ParseNovel(Novel, cancellationToken);
                }
            }
            catch (Exception ex)

            {
                MessageBoxHelper.ShowErrorWindow(ex.Message);
            }
        }

        public bool CanStartButtonClick => commonNovelParser.ValidateUrl(NovelLink);

        private async Task GetNovelInfo(CancellationToken cancellationToken)
        {
            Novel = await commonNovelParser.ParseCommonInfo(NovelLink, cancellationToken);
            if (Novel != null && !string.IsNullOrEmpty(Novel.Name))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                SavePath = Path.Combine(desktop, NovelParserBLL.Utilities.FileHelper.RemoveInvalidFilePathCharacters(Novel.Name));
                SelectedTranslationTeam = TranslationTeams.First();
            }
        }

        private async Task ParseNovel(Novel novel, CancellationToken cancellationToken)
        {
            ProgressValueProgressButton = 0;
            await commonNovelParser.LoadChapters(novel, SelectedTranslationTeam, ListChaptersPattern, IncludeImages, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            await fileGeneratorService.Generate(SavePath, GetSelectedFileFormat(), novel, SelectedTranslationTeam, ListChaptersPattern);
        }

        private void SetProgressValueProgressButton(int total, int current)
        {
            ProgressValueProgressButton = (int)(current / (double)total * 100);
        }

        [GenerateCommand]
        private void StartParse()
        {
            IsLoadingProgressButton = true;
        }

        #endregion Parsing

        #region LinksCommands

        [GenerateCommand]
        private void OpenGitHub() => UrlHelper.OpenUrlInDefaultBrowser("https://github.com/Relorer/NovelParser");

        public Dictionary<string, string> ParserURLs { get; }

        [GenerateCommand]
        private void OpenSiteOfParser(string url) => UrlHelper.OpenUrlInDefaultBrowser(url);

        #endregion LinksCommands

        #region CookiesSettings

        public bool UseCookies
        {
            get => bool.Parse(ConfigurationManager.AppSettings["UseCookies"] ?? "false");
            set
            {
                SettingsHelper.AddOrUpdateAppSettings("UseCookies", value.ToString());
            }
        }

        private ChromeDriver? authDriver;

        [GenerateCommand]
        private void OpenAuthClick(string url)
        {
            TryCloseAuthDriver();
            authDriver = ChromeDriverHelper.OpenPageWithAutoClose(url);
        }

        private bool CanOpenAuthClick(string url) => UseCookies;

        [GenerateCommand]
        private void ClearCookiesClick()
        {
            TryCloseAuthDriver();
            ChromeDriverHelper.ClearCookies();
        }

        private bool CanClearCookiesClick() => UseCookies;

        private void TryCloseAuthDriver()
        {
            authDriver?.Dispose();
            authDriver = null;
        }

        #endregion CookiesSettings

        [GenerateCommand]
        private void ClearCacheClick()
        {
            novelCacheService.ClearCache();
        }

        #region CloseSettings

        [GenerateCommand]
        private void CloseSettingsHandler()
        {
            TryCloseAuthDriver();
        }

        #endregion CloseSettings

        #region CloseWindow

        [GenerateCommand]
        private void CloseWindowHandler()
        {
            TryCloseAuthDriver();
            cancellationTokenSource?.Dispose();
        }

        #endregion CloseWindow

        private FileFormat GetSelectedFileFormat()
        {
            return (FileFormat)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }
    }
}