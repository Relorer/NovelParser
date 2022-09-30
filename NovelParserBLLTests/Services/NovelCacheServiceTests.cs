using Microsoft.VisualStudio.TestTools.UnitTesting;
using NovelParserBLLTests.Models;

namespace NovelParserBLL.Services.Tests
{
    [TestClass()]
    public class NovelCacheServiceTests
    {
        [TestMethod()]
        public void SaveAndGetNovelTest()
        {
            var novelCacheService = new NovelCacheService();

            var novel = TestNovel.GetTestNovel();

            novelCacheService.SaveNovelToFile(novel);
            var cacheNovel = novelCacheService.TryGetNovelFromFile(novel.URL!)!;


            Assert.AreEqual(novel.Name, cacheNovel.Name);
            Assert.AreEqual(novel.Description, cacheNovel.Description);
            Assert.AreEqual(novel.Cover!.Length, cacheNovel.Cover!.Length);
            Assert.AreEqual(novel.Cover[0], cacheNovel.Cover[0]);
            Assert.AreEqual(novel.Author, cacheNovel.Author);

            Assert.AreEqual(novel.ChaptersByGroup!.Count, cacheNovel.ChaptersByGroup!.Count);
            Assert.AreEqual(
                novel.ChaptersByGroup!.First().Value.First().Value.Name,
                cacheNovel.ChaptersByGroup!.First().Value.First().Value.Name
                );
        }
    }
}