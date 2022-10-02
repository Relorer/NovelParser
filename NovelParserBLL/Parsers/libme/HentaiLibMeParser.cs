using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class HentaiLibMeParser : ComicsLibMeParser
    {
        public HentaiLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override string SiteName => "HentaiLib.me";

        public override string SiteDomen => "https://hentailib.me/";

        protected override List<string> servers => new List<string>()
        {
            "https://img2.hentailib.org",
            "https://img3.hentailib.org",
            "https://img4.hentailib.org",
        };
    }
}