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
        private bool isParsing = false;
        public bool IsParsing
        {
            get => isParsing;
            set
            {
                isParsing = value;
                NovelLoadingLock = value;
                Console.WriteLine(isParsing);
            }
        }

        public bool NovelLoadingLock { get; set; } = false;
        public int Progress { get; set; } = 55;

        [AsyncCommand]
        public async Task StartButtonClick()
        {
            NovelLoadingLock = true;

            TryCloseAuthDriver();

            try
            {
                if (Novel == null)
                {
                    await GetNovelInfo();
                }
                else
                {
                    await ParseNovel(Novel);
                }
            }
            catch (WebDriverArgumentException)
            {
            }
            catch (Exception)
            {
            }

            NovelLoadingLock = false;
        }
        public bool CanStartButtonClick() => ranobelib.ValidateUrl(NovelLink);

        private async Task GetNovelInfo()
        {
            Novel = await ranobelib.ParseAsync(NovelLink);
            if (Novel != null && !string.IsNullOrEmpty(Novel.NameEng))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var format = GetSelectedFileFormat() == FileFormatForGenerator.EPUB ? ".epub" : ".pdf";
                SavePath = Path.Combine(desktop, FileSystemHelper.RemoveInvalidFilePathCharacters(Novel.NameEng) + format);
                SelectedTranslationTeam = TranslationTeams.First();
            }
        }
        private async Task ParseNovel(Novel novel)
        {
            await ranobelib.ParseAndLoadChapters(ChaptersToDownload, IncludeImages);
            await new EpubFileGenerator().Generate(SavePath, novel, ChaptersToDownload);
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

        #region CloseWindow

        [GenerateCommand]
        private void CloseWindowHandler()
        {
            TryCloseAuthDriver();
        }

        #endregion

        private FileFormatForGenerator GetSelectedFileFormat()
        {
            return (FileFormatForGenerator)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }
    }
}