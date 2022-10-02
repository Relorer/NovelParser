using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class MangaLibMeParser : ComixLibMeParser
    {
        public MangaLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        protected override string domen => "https://mangalib.me/";
    }
}