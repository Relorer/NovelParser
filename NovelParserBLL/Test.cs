using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.Parsers;
using NovelParserBLL.Parsers.Ranobelib;

namespace NovelParserBLL
{
    public class Test
    {
        public async void StartTest()
        {
            const string input = "https://ranobelib.me/odinnadcatyi-priklyuceniya-proklyatogo/v1/c1";

            INovelParser parser = new RanobelibParser();
            var fileGenerator = new EpubFileGenerator();

            var novel = await parser.ParseAsync(input);

            await parser.ParseAndLoadChapters(novel);

            fileGenerator.Generate("test.epub", novel);

        }

    }
}