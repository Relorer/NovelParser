using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using NovelParserBLL.Models;
using NovelParserBLL.Services;
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
        public MainWindowViewModel()
        {
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

        public List<string> TranslationTeams => Novel?.ChaptersByTranslationTeam?.Keys.ToList() ?? new List<string>();

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;

        private SortedList<int, Chapter>? chaptersCurrentTeam => Novel?.ChaptersByTranslationTeam?.GetValueOrDefault(SelectedTranslationTeam, new SortedList<int, Chapter>());

        public SortedList<int, Chapter> ChaptersToDownload => ChapterHelper.GetChaptersByPattern(ListChaptersPattern, chaptersCurrentTeam);

        public BitmapImage? Cover => Novel?.Cover == null ? null : ImageHelper.BitmapImageFromBuffer(Novel.Cover);

        #region ParsingParams

        private string novelLink = "";
        public string NovelLink
        {
            get => novelLink;
            set
            {
                novelLink = value;
                Novel = null;
            }
        }

        public string SelectedTranslationTeam { get; set; } = "";

        public string ListChaptersPattern { get; set; } = "All";

        public bool IncludeImages { get; set; } = true;

        public string SavePath { get; set; } = "";

        public string FileName => Path.GetFileName(SavePath);

        [GenerateCommand]
        private void SelectSavePath()
        {
            SavePath = FileDialogHelper.GetSaveFilePath(SavePath, GetSelectedFileFormat());
        }
        private bool CanSelectSavePath() => Novel != null;

        #endregion

        #region Parsing

        public string ProgressButtonText { get; set; } = "Start";
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

        public async Task StartButtonClick(CancellationToken cancellationToken)
        {
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

        public bool CanStartButtonClick => NovelParserService.ValidateUrl(NovelLink);

        private async Task GetNovelInfo(CancellationToken cancellationToken)
        {
            Novel = await NovelParserService.ParseAsync(NovelLink, cancellationToken);
            if (Novel != null && !string.IsNullOrEmpty(Novel.NameEng))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                SavePath = Path.Combine(desktop, NovelParserBLL.Utilities.FileHelper.RemoveInvalidFilePathCharacters(Novel.NameEng));
                SelectedTranslationTeam = TranslationTeams.First();
            }
        }

        private async Task ParseNovel(Novel novel, CancellationToken cancellationToken)
        {
            ProgressValueProgressButton = 0;
            await NovelParserService.ParseAndLoadChapters(novel, ChaptersToDownload, IncludeImages, SetProgressValueProgressButton, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            await FileGeneratorService.Generate(SavePath, GetSelectedFileFormat(), novel, ChaptersToDownload);
        }

        private void SetProgressValueProgressButton(int total, int current)
        {
            ProgressValueProgressButton = (int)(current / (double)total * 100);
        }

        [GenerateCommand]
        void StartParse()
        {
            IsLoadingProgressButton = true;
        }

        #endregion

        #region LinksCommands

        [GenerateCommand]
        private void OpenGitHub() => UrlHelper.OpenUrlInDefaultBrowser("https://github.com/Relorer/NovelParser");

        [GenerateCommand]
        private void OpenRanobeLib() => UrlHelper.OpenUrlInDefaultBrowser("https://ranobelib.me/");

        #endregion

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
        private void OpenRanobeLibAuthClick()
        {
            TryCloseAuthDriver();
            authDriver = ChromeDriverHelper.OpenPageWithAutoClose("https://lib.social/login");
        }
        private bool CanOpenRanobeLibAuthClick() => UseCookies;

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

        #endregion

        [GenerateCommand]
        private void ClearCacheClick()
        {
            NovelCacheService.ClearCache();
        }

        #region CloseWindow

        [GenerateCommand]
        private void CloseWindowHandler()
        {
            TryCloseAuthDriver();
            cancellationTokenSource?.Dispose();
        }

        #endregion

        private FileFormat GetSelectedFileFormat()
        {
            return (FileFormat)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }
    }
}