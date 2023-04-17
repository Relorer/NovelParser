using Microsoft.VisualStudio.TestTools.UnitTesting;
using NovelParserBLL.Services;
using NovelParserBLLTests.Models;

namespace NovelParserBLLTests.Services;

[TestClass]
public class NovelCacheServiceTests
{
    [TestMethod]
    public void SaveAndGetNovelTest()
    {
        var novelCacheService = new NovelCacheService();

        var novel = TestNovel.GetTestNovel();

        novelCacheService.SaveNovelToFile(novel);
        var cacheNovel = novelCacheService.TryGetNovelFromFile(novel.URL!)!;

        Assert.AreEqual(novel.Name, cacheNovel.Name);
        Assert.AreEqual(novel.Description, cacheNovel.Description);
        Assert.AreEqual(novel.Cover, cacheNovel.Cover);
        Assert.AreEqual(novel.Author, cacheNovel.Author);

        Assert.AreEqual(novel.ChaptersByGroup!.Count, cacheNovel.ChaptersByGroup!.Count);
        Assert.AreEqual(
            novel.ChaptersByGroup!.First().Value.First().Name,
            cacheNovel.ChaptersByGroup!.First().Value.First().Name
        );
    }
}