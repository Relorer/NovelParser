using NovelParserBLL.Comparer;
using NovelParserBLL.Models;

namespace NovelParserBLL.Extensions;

public static class NovelExtensions
{
    public static void Merge(this Novel first, Novel second)
    {
        first.URL ??= second.URL;
        first.Name ??= second.Name;
        first.Author ??= second.Author;
        first.Description ??= second.Description;
        first.Cover ??= second.Cover;

        if (first.ChaptersByGroup != null && second.ChaptersByGroup != null)
        {
            foreach (var team in second.ChaptersByGroup)
            {
                if (first.ChaptersByGroup.TryGetValue(team.Key, out var chapters))
                {
                    foreach (var item in team.Value
                                 .Where(item => !chapters.Contains(item, new ChaptersEqualityComparer())))
                    {
                        chapters.Add(item);
                    }
                }
                else
                {
                    first.ChaptersByGroup.Add(team.Key, team.Value);
                }
            }
        }
        else
        {
            first.ChaptersByGroup ??= second.ChaptersByGroup;
        }
    }
}
