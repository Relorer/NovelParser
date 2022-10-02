using NovelParserBLL.Models;

namespace NovelParserBLLTests.Models
{
    internal static class TestNovel
    {
        public static Novel GetTestNovel()
        {
            return new Novel()
            {
                Name = "Test",
                URL = "Test",
                Description = "Test",
                Cover = new ImageInfo("test"),
                Author = "Test",
                ChaptersByGroup = new Dictionary<string, SortedList<int, Chapter>>()
                {
                    {
                        "test",
                        new SortedList<int, Chapter>()
                        {
                            {1, new Chapter() {Name = "Test"} }
                        }
                    }
                }
            };
        }
    }
}