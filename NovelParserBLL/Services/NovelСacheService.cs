using Newtonsoft.Json;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Services
{
    public static class NovelCacheService
    {
        private static string cacheFolder = Path.Combine(Directory.GetCurrentDirectory(), "Cache");

        static NovelCacheService()
        {
            if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);
        }

        public static void SaveNovelToFile(Novel novel)
        {
            string json = JsonConvert.SerializeObject(novel, Formatting.Indented);
            var path = Path.Combine(cacheFolder, FileHelper.RemoveInvalidFilePathCharacters(novel.URL) + ".json");
            File.WriteAllText(path, json);
        }

        public static Novel? TryGetNovelFromFile(string url)
        {
            var path = Path.Combine(cacheFolder, FileHelper.RemoveInvalidFilePathCharacters(url) + ".json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Novel?>(json);
            }
            return null;
        }

        public static void ClearCache()
        {
            if (Directory.Exists(cacheFolder))
            {
                DirectoryHelper.Empty(cacheFolder);
            }
        }
    }
}
