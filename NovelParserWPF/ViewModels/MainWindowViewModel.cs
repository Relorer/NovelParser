using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using Microsoft.Win32;
using NovelParserBLL.Extensions;
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
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
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
        string novelLink = "";

        public string NovelLink
        {
            get => novelLink;
            [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(novelLink))]
            set
            {
                if (EqualityComparer<string>.Default.Equals(novelLink, value)) return;
                novelLink = value;
                ListChaptersPattern = "All";
                Novel = null;
            }
        }

        public BitmapImage? Cover => Novel?.Cover == null ? null : ImageHelper.BitmapImageFromBuffer(Novel.Cover);

        [GenerateProperty]
        bool includeImages = true;

        [GenerateProperty]
        List<RadioButton> fileFormatsForGenerator;

        [GenerateProperty]
        string selectedTranslationTeam = "";

        public List<string> TranslationTeams => Novel?.ChaptersByTranslationTeam?.Keys.ToList() ?? new List<string>();

        [GenerateProperty]
        string listChaptersPattern = "All";

        [GenerateProperty]
        bool novelLoadingLock = false;

        [GenerateCommand]
        void OpenGitHub() => Process.Start(new ProcessStartInfo() { FileName = "https://github.com/Relorer/NovelParser", UseShellExecute = true });

        [GenerateCommand]
        void OpenRanobeLib() => Process.Start(new ProcessStartInfo() { FileName = "https://ranobelib.me/", UseShellExecute = true });

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

        public bool UseCookies
        {
            get => bool.Parse(ConfigurationManager.AppSettings["UseCookies"] ?? "false");
            set
            {
                SettingsHelper.AddOrUpdateAppSettings("UseCookies", value.ToString());
            }
        }

        [GenerateProperty]
        Novel? novel;

        private SortedList<int, Chapter>? chaptersCurrentTeam => Novel?.ChaptersByTranslationTeam?.GetValueOrDefault(SelectedTranslationTeam, new SortedList<int, Chapter>());

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;
        public SortedList<int, Chapter> ChaptersToDownload => GetChaptersByPattern(ListChaptersPattern, chaptersCurrentTeam);

        [GenerateProperty]
        string savePath = "";

        public string FileName => Path.GetFileName(SavePath);

        [GenerateCommand]
        void SelectSavePath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Epub file|*.epub";
            saveFileDialog.Title = "Save an EPUB File";
            saveFileDialog.FileName = SavePath;
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                SavePath = saveFileDialog.FileName;
            }
        }
        bool CanSelectSavePath() => Novel != null;

        #region SettingsRegion

        private ChromeDriver? authDriver;

        [GenerateCommand]
        public void OpenRanobeLibAuthClick()
        {
            TryCloseAuthDriver();
            authDriver = ranobelib.OpenAuthPage();
        }

        private void TryCloseAuthDriver()
        {
            authDriver?.Dispose();
            authDriver = null;
        }

        public bool CanOpenRanobeLibAuthClick() => UseCookies;

        [GenerateCommand]
        void CloseWindowHandler()
        {
            TryCloseAuthDriver();
        }

        [GenerateCommand]
        void ClearCookiesClick()
        {
            TryCloseAuthDriver();
            ChromeDriverHelper.ClearCookies();
        }
        bool CanClearCookiesClick() => UseCookies;

        #endregion

        private RanobelibParser ranobelib = new RanobelibParser();

        public MainWindowViewModel()
        {
            InitFileFormatList();
        }

        private FileFormatForGenerator GetSelectedFileFormat()
        {
            return (FileFormatForGenerator)FileFormatsForGenerator.Find(format => format.IsChecked ?? false)!.Content;
        }

        private void InitFileFormatList()
        {
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

        private SortedList<int, Chapter> GetChaptersByPattern(string pattern, SortedList<int, Chapter>? chapters)
        {
            var result = new SortedList<int, Chapter>(chapters?.Count ?? 0);

            if (chapters == null) return result;

            var parts = pattern.RemoveWhiteSpaces().ToLower().Split(',');

            var addRange = (int start, int end) =>
            {
                if (end < start) (start, end) = (end, start);
                start = Math.Max(1, start);
                end = Math.Min(chapters.Last().Key, end);

                for (int i = start; i <= end; i++)
                {
                    if (!result.ContainsKey(i) && chapters.TryGetValue(i, out Chapter? ch))
                    {
                        result.Add(i, ch);
                    }
                }
            };

            foreach (var part in parts)
            {
                if (part.Equals("all"))
                {
                    return chapters;
                }
                if (part.Contains('-'))
                {
                    var nums = part.Split('-');

                    bool containsNum1 = int.TryParse(nums[0], out int num1);
                    bool containsNum2 = int.TryParse(nums[1], out int num2);

                    addRange(containsNum1 ? num1 : 1, containsNum2 ? num2 : chapters.Count);
                }
                else if (int.TryParse(part, out int num))
                {
                    var index = Math.Min(chapters.Last().Key, num);
                    if (chapters.TryGetValue(index, out Chapter? ch))
                    {
                        result.Add(index, ch);
                    }
                }
            }

            return result;
        }

        static PropertyChangedEventArgs NovelLinkChangedEventArgs = new PropertyChangedEventArgs(nameof(NovelLink));
    }
}
