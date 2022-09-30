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
                Cover = new byte[1] { 1 },
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
