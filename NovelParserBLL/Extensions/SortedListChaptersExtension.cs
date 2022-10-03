using NovelParserBLL.Models;

namespace NovelParserBLL.Extensions
{
    internal static class SortedListChaptersExtension
    {
        public static List<Chapter> ForLoad(this SortedList<int, Chapter> chapters, bool includeImages)
        {
            return chapters.Select(v => v.Value).Where(ch => string.IsNullOrEmpty(ch.Content) || (!ch.ImagesLoaded || !ch.Images.Exists()) && includeImages).ToList();
        }
    }
}