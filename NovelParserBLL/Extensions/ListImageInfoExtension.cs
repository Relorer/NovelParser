using NovelParserBLL.Models;

namespace NovelParserBLL.Extensions
{
    internal static class ListImageInfoExtension
    {
        public static bool Exists(this List<ImageInfo> images)
        {
            return images.Select(img => img.Exists).Aggregate((img1, img2) => img1 && img2);
        }
    }
}