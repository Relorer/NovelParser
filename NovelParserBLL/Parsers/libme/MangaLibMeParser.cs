using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class MangaLibMeParser : ComicsLibMeParser
    {
        public MangaLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override string SiteDomen => "https://mangalib.me/";

        public override string SiteName => "MangaLib.me";
    }
}