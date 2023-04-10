using Newtonsoft.Json;
using NovelParserBLL.Extensions;
using NovelParserBLL.Properties;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string? Author { get; set; }
        public Dictionary<string, SortedList<float, Chapter>>? ChaptersByGroup { get; set; }
        public ImageInfo? Cover { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [JsonIgnore]
        public string DownloadFolderName => Path.Combine(Resources.CacheFolder, FileHelper.RemoveInvalidFilePathCharacters(URL ?? ""));

        public SortedList<float, Chapter> this[string? group, string? pattern]
        {
            get
            {
                if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(pattern) ||
                    (!ChaptersByGroup?.ContainsKey(group) ?? false)) return new SortedList<float, Chapter>();

                var chapters = ChaptersByGroup![group];

                var result = new SortedList<float, Chapter>(chapters?.Count ?? 0);

                if (chapters == null) return result;

                var parts = pattern.RemoveWhiteSpaces().ToLower().Split(',');

                foreach (var part in parts)
                {
                    if (part.Equals("all"))
                    {
                        return chapters;
                    }
                    if (part.Contains('-'))
                    {
                        var nums = part.Split('-');

                        var containsNum1 = float.TryParse(nums[0], out var num1);
                        var containsNum2 = float.TryParse(nums[1], out var num2);

                        var start = containsNum1 ? num1 : 1;
                        var end = containsNum2 ? num2 : chapters.Last().Key;
                        if (end < start) (start, end) = (end, start);
                        start = Math.Max(1, start);
                        end = Math.Min(chapters.Last().Key, end);

                        var list = chapters.Where(ch => ch.Key >= start && ch.Key <= end);
                        foreach (var item in list)
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                    else if (float.TryParse(part, out var num))
                    {
                        var index = Math.Min(chapters.Last().Key, num);
                        if (chapters.TryGetValue(index, out var ch))
                        {
                            result.Add(index, ch);
                        }
                    }
                }

                return result;
            }
        }

        public void Merge(Novel secondNovelInfo)
        {
            URL ??= secondNovelInfo.URL;
            Name ??= secondNovelInfo.Name;
            Author ??= secondNovelInfo.Author;
            Description ??= secondNovelInfo.Description;
            Cover ??= secondNovelInfo.Cover;

            if (ChaptersByGroup != null && secondNovelInfo.ChaptersByGroup != null)
            {
                foreach (var team in secondNovelInfo.ChaptersByGroup)
                {
                    if (ChaptersByGroup.TryGetValue(team.Key, out var chapters))
                    {
                        foreach (var item in team.Value)
                        {
                            if (!chapters.ContainsKey(item.Key))
                                chapters.Add(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        ChaptersByGroup.Add(team.Key, team.Value);
                    }
                }
            }
            else
            {
                ChaptersByGroup ??= secondNovelInfo.ChaptersByGroup;
            }
        }
    }
}