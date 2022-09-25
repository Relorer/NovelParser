using NovelParserBLL.Extensions;

namespace NovelParserBLL.Models
{
    public class Novel
    {
        public string? Author { get; set; }
        public Dictionary<string, SortedList<int, Chapter>>? ChaptersByGroup { get; set; }
        public byte[]? Cover { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        public SortedList<int, Chapter> this[string? group, string? pattern]
        {
            get
            {
                if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(pattern) ||
                    (!ChaptersByGroup?.ContainsKey(group) ?? false)) return new SortedList<int, Chapter>();

                var chapters = this.ChaptersByGroup![group];

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

                        addRange(containsNum1 ? num1 : 1, containsNum2 ? num2 : chapters.Last().Key);
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
        }

        public void Merge(Novel secondNovelInfo)
        {
            this.URL = this.URL ?? secondNovelInfo.URL;
            this.Name = this.Name ?? secondNovelInfo.Name;
            this.Author = this.Author ?? secondNovelInfo.Author;
            this.Description = this.Description ?? secondNovelInfo.Description;
            this.Cover = this.Cover ?? secondNovelInfo.Cover;

            if (this.ChaptersByGroup != null && secondNovelInfo.ChaptersByGroup != null)
            {
                foreach (var team in secondNovelInfo.ChaptersByGroup)
                {
                    if (this.ChaptersByGroup.TryGetValue(team.Key, out SortedList<int, Chapter>? chapters))
                    {
                        foreach (var item in team.Value)
                        {
                            if (!chapters.TryGetValue(item.Key, out Chapter? chapter))
                            {
                                chapters.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else
                    {
                        this.ChaptersByGroup.Add(team.Key, team.Value);
                    }
                }
            }
            else
            {
                this.ChaptersByGroup = this.ChaptersByGroup ?? secondNovelInfo.ChaptersByGroup;
            }
        }
    }
}