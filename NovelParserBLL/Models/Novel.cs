using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Properties;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string? Author { get; set; }
        public Dictionary<string, List<Chapter>>? ChaptersByGroup { get; set; }
        public ImageInfo? Cover { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [JsonIgnore]
        public string DownloadFolderName => Path.Combine(Resources.CacheFolder, FileHelper.RemoveInvalidFilePathCharacters(URL ?? ""));

        //todo Переработать с учетом томов
        public List<Chapter> this[string? group, string? pattern]
        {
            get
            {
                var result = new List<Chapter>();

                if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(pattern) ||
                    (!ChaptersByGroup?.ContainsKey(group) ?? false))
                    return result;

                var chapters = ChaptersByGroup?[group];
                if (chapters == null)
                    return result;

                return chapters.SortChapters();
                //var lastChapterIndex = chapters.Count - 1;
                //var parts = pattern.RemoveWhiteSpaces().ToLower().Split(',');

                //foreach (var part in parts)
                //{
                //    if (part.Equals("all")) return chapters;

                //    if (part.Contains('-'))
                //    {
                //        var nums = part.Split('-');
                //        var containsNum1 = int.TryParse(nums[0], out var num1);
                //        var containsNum2 = int.TryParse(nums[1], out var num2);


                //        var start = containsNum1 ? num1 : 1;
                //        var end = containsNum2 ? num2 : lastChapterIndex;

                //        if (end < start) (start, end) = (end, start);

                //        start = Math.Max(1, start);
                //        end = Math.Min(lastChapterIndex, end);

                //        var list = chapters.Where(ch => ch.Id >= start && ch.Id <= end);
                //        result.AddRange(list);
                //    }
                //    else if (int.TryParse(part, out var num))
                //    {
                //        var index = Math.Min(lastChapterIndex, num);
                //        result.Add(chapters[index]);
                //    }
                //}

                //return result.OrderBy(c => c, new ChaptersComparer()).ToList();
            }
        }
    }
}