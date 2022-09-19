using DevExpress.Mvvm.CodeGenerators;
using Microsoft.Win32;
using NovelParserBLL.Extensions;
using NovelParserBLL.FileGenerators;
using NovelParserBLL.Models;
using NovelParserBLL.Parsers.Ranobelib;
using NovelParserBLL.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace NovelParserWPF.ViewModels
{
    [GenerateViewModel]
    public partial class MainWindowViewModel
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
                RaisePropertyChanged(NovelLinkChangedEventArgs);
                ListChaptersPattern = "All";
                Novel = null;
            }
        }

        [GenerateProperty]
        bool novelLinkLock = false;

        [GenerateProperty]
        bool includeImages = true;

        [GenerateProperty]
        List<RadioButton> fileFormatsForGenerator;

        [GenerateProperty]
        string selectedTranslationTeam = "";

        public List<string> TranslationTeam => Novel?.ChaptersByTranslationTeam?.Keys.ToList() ?? new List<string>();

        [GenerateProperty]
        string listChaptersPattern = "All";

        [GenerateCommand]
        void OpenGitHub() => Process.Start(new ProcessStartInfo() { FileName = "https://github.com/Relorer/NovelParser", UseShellExecute = true });

        [GenerateCommand]
        void OpenRanobeLib() => Process.Start(new ProcessStartInfo() { FileName = "https://ranobelib.me/", UseShellExecute = true });

        [GenerateCommand]
        async void StartButtonClick()
        {
            NovelLinkLock = true;
            Novel = await ranobelib.ParseAsync(NovelLink);
            if (Novel != null && !string.IsNullOrEmpty(Novel.NameEng))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                SavePath = Path.Combine(desktop, FileSystemHelper.RemoveInvalidFilePathCharacters(Novel.NameEng));
                SelectedTranslationTeam = TranslationTeam.First();
            }
            NovelLinkLock = false;
        }
        bool CanStartButtonClick() => ranobelib.ValidateUrl(NovelLink);

        [GenerateProperty]
        Novel? novel;

        private List<Chapter>? chaptersCurrentTeam => Novel?.ChaptersByTranslationTeam?.GetValueOrDefault(SelectedTranslationTeam, new List<Chapter>());

        public int TotalChapters => chaptersCurrentTeam?.Count ?? 0;
        public int ChaptersToDownload => GetChaptersByPattern(ListChaptersPattern, chaptersCurrentTeam).Count();

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

        [GenerateProperty]
        bool useCookies = false;

        [GenerateCommand]
        void OpenRanobeLibAuthClick() => Console.WriteLine("Open ranobelib auth");
        bool CanOpenRanobeLibAuthClick() => UseCookies;

        [GenerateCommand]
        void ClearCookiesClick() => Console.WriteLine("ClearCookiesClick");
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

        private IEnumerable<Chapter> GetChaptersByPattern(string pattern, List<Chapter>? chapters)
        {
            var result = new List<Chapter>(chapters?.Count ?? 0);

            if (chapters == null) return result;

            var parts = pattern.RemoveWhiteSpaces().ToLower().Split(',');

            var addRange = (int start, int end) =>
            {
                start -= start > 0 ? 1 : 0;
                if (end < start) (start, end) = (end, start);
                end = Math.Min(chapters.Count, end);
                result.AddRange(chapters.GetRange(start, end - start));
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
                    result.Add(chapters[Math.Min(chapters.Count - 1, num)]);
                }
            }

            return result.Distinct();
        }

        static PropertyChangedEventArgs NovelLinkChangedEventArgs = new PropertyChangedEventArgs(nameof(NovelLink));
    }
}
