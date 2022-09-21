﻿using NovelParserBLL.Extensions;

namespace NovelParserBLL.Models
{
    public class Chapter
    {
        public string Name { get; }
        public string Url { get; }
        public string Number { get; }
        public int Index { get; }
        public string Content { get; set; }
        public bool ImagesLoaded { get; set; }

        public Chapter(string name, string url, string content, string number, int index)
        {
            Name = name;
            Url = url;
            Content = content;
            Number = number;
            Index = index;
        }

        public static SortedList<int, Chapter> GetChaptersByPattern(string pattern, SortedList<int, Chapter>? chapters)
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
    }
}
