using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using NovelParserBLL.FileGenerators;
using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.Models;
using NovelParserBLL.Parsers.Ranobelib;
using NovelParserBLL.Utilities;
using NovelParserWPF.Utilities;
using OpenQA.Selenium;
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
        private readonly RanobelibParser ranobelib;

        public MainWindowViewModel()
        {
            ranobelib = new RanobelibParser();

            FileFormatsForGenerator = new List<RadioButton>() {
                new RadioButton() {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormatForGenerator.EPUB,
                    IsChecked = true
                },
                new RadioButton() {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormatForGenerator.PDF,
                    IsEnabled = false
                }
            };
        }

        public List<RadioButton> FileFormatsForGenerator { get; set; }

        public Novel? Novel { get; set; }

        public List<string> TranslationTeams => Novel?.ChaptersByTranslationTeam?.Keys.ToList() ?? new List<string>();

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;

        private SortedList<int, Chapter>? chaptersCurrentTeam => Novel?.ChaptersByTranslationTeam?.GetValueOrDefault(SelectedTranslationTeam, new SortedList<int, Chapter>());

        public SortedList<int, Chapter> ChaptersToDownload => Chapter.GetChaptersByPattern(ListChaptersPattern, chaptersCurrentTeam);

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
            SavePath = FileHelper.GetSaveFilePath(SavePath, GetSelectedFileFormat());
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
            catch (WebDriverArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool CanStartButtonClick => ranobelib.ValidateUrl(NovelLink);

        private async Task GetNovelInfo(CancellationToken cancellationToken)
        {
            Novel = await ranobelib.ParseAsync(NovelLink, cancellationToken);
            if (Novel != null && !string.IsNullOrEmpty(Novel.NameEng))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var format = GetSelectedFileFormat() == FileFormatForGenerator.EPUB ? ".epub" : ".pdf";
                SavePath = Path.Combine(desktop, FileSystemHelper.RemoveInvalidFilePathCharacters(Novel.NameEng) + format);
                SelectedTranslationTeam = TranslationTeams.First();
            }
        }
        private async Task ParseNovel(Novel novel, CancellationToken cancellationToken)
        {
            await ranobelib.ParseAndLoadChapters(novel, ChaptersToDownload, IncludeImages, SetProgressValueProgressButton, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            await new EpubFileGenerator().Generate(SavePath, novel, ChaptersToDownload);
        }

        private void SetProgressValueProgressButton(int total, int current)
        {
            ProgressValueProgressButton = (int)(current / (double)total * 100);
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
            authDriver = ranobelib.OpenAuthPage();
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

        private FileFormatForGenerator GetSelectedFileFormat()
        {
            return (FileFormatForGenerator)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }
    }
}