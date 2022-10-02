using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class HentaiLibMeParser : ComixLibMeParser
    {
        public HentaiLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        protected override string domen => "https://hentailib.me/";

        protected override List<string> servers => new List<string>()
        {
            "https://img2.hentailib.org",
            "https://img3.hentailib.org",
            "https://img4.hentailib.org",
        };
    }
}