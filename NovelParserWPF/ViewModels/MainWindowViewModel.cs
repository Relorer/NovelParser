using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using NovelParserBLL.FileGenerators;
using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.FileGenerators.PDF;
using NovelParserBLL.Models;
using NovelParserBLL.Parsers;
using NovelParserBLL.Services;
using NovelParserWPF.DialogWindows;
using NovelParserWPF.Utilities;
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

            ParserInfos = commonNovelParser.ParserInfos;

            FileFormatsForGenerator = new List<RadioButton>() {
                new RadioButton {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormat.EPUB,
                    IsChecked = true
                },
                new RadioButton {
                    GroupName = nameof(FileFormatsForGenerator),
                    Content = FileFormat.PDF,
                }
            };
        }

        public List<RadioButton> FileFormatsForGenerator { get; set; }

        public Novel? Novel { get; set; }

        public List<string> TranslationTeams => Novel?.ChaptersByGroup?.Keys.ToList() ?? new List<string>();

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;

        private SortedList<float, Chapter>? chaptersCurrentTeam => Novel?[SelectedTranslationTeam, "all"];

        public SortedList<float, Chapter>? ChaptersToDownload => Novel?[SelectedTranslationTeam, ListChaptersPattern];

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
                    loadingTask.ContinueWith( _ =>
                    {
                        IsLoadingProgressButton = false;
                    });
                }
            }
        }

        private async Task StartButtonClick(CancellationToken cancellationToken)
        {
            if (!commonNovelParser.ValidateUrl(NovelLink)) return;

            try
            {
                //TryCloseAuthDriver();

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
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                SavePath = Path.Combine(desktop, NovelParserBLL.Utilities.FileHelper.RemoveInvalidFilePathCharacters(Novel.Name));
                SelectedTranslationTeam = TranslationTeams.First();
            }
        }

        private async Task ParseNovel(Novel novel, CancellationToken cancellationToken)
        {
            ProgressValueProgressButton = 0;
            await commonNovelParser.LoadChapters(novel, SelectedTranslationTeam, ListChaptersPattern, IncludeImages, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            var fileFormat = GetSelectedFileFormat();

            GenerationParams generationParams = fileFormat switch
            {
                FileFormat.EPUB => new EPUBGenerationParams(SavePath, novel, SelectedTranslationTeam, ListChaptersPattern),
                FileFormat.PDF => new PDFGenerationParams(SavePath, novel, SelectedTranslationTeam, ListChaptersPattern, PDFType.LongPage),
                _ => throw new NotImplementedException(),
            };

            await fileGeneratorService.Generate(generationParams);
        }

        private void SetProgressValueProgressButton(int total, int current, string status)
        {
            ProgressValueProgressButton = (int)(current / (double)total * 100);
            ProgressButtonText = status;
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

        public List<ParserInfo> ParserInfos { get; }

        [GenerateCommand]
        private void OpenSiteOfParser(string url) => UrlHelper.OpenUrlInDefaultBrowser(url);

        #endregion LinksCommands

        #region CookiesSettings

        public bool UseCookies
        {
            get => bool.Parse(ConfigurationManager.AppSettings["UseCookies"] ?? "false");
            set => SettingsHelper.AddOrUpdateAppSettings("UseCookies", value.ToString());
        }

        //private ChromeDriver? authDriver;

        [GenerateCommand]
        private void OpenAuthClick(string url)
        {
            //TryCloseAuthDriver();
            //authDriver = ChromeDriverHelper.OpenPageWithAutoClose(url);
        }

        private bool CanOpenAuthClick(string url) => UseCookies;

        [GenerateCommand]
        private void ClearCookiesClick()
        {
            //TryCloseAuthDriver();
            //ChromeDriverHelper.ClearCookies();
        }

        private bool CanClearCookiesClick() => UseCookies;

        //private void TryCloseAuthDriver()
        //{
        //    authDriver?.Dispose();
        //    authDriver = null;
        //}

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
            //TryCloseAuthDriver();
        }

        #endregion CloseSettings

        #region CloseWindow

        [GenerateCommand]
        private void CloseWindowHandler()
        {
            //TryCloseAuthDriver();
            cancellationTokenSource?.Dispose();
        }

        #endregion CloseWindow

        private FileFormat GetSelectedFileFormat()
        {
            return (FileFormat)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }
    }
}