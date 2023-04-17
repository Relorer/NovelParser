using NovelParserBLL.Comparer;
using NovelParserBLL.Models;

namespace NovelParserBLL.Extensions;

public static class ListChaptersExtension
{
    public static List<Chapter> ForLoad(this List<Chapter> chapters, bool includeImages)
    {
        return chapters.Where(ch => 
                string.IsNullOrEmpty(ch.Content) || (!ch.ImagesLoaded || !ch.Images.Exists()) && includeImages)
            .ToList();
    }

    public static List<Chapter> SortChapters(this List<Chapter> chapters)
    {
        return chapters.OrderBy(c => c, new ChaptersComparer()).ToList();
    }

    public static int VolumesCount(this List<Chapter> chapters)
    {
        return chapters.Select(c => c.Volume).ToHashSet().Count;
    }
}