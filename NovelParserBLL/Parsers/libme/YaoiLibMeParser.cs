using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class YaoiLibMeParser : ComicsLibMeParser
    {
        public YaoiLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        public override string SiteDomen => "https://yaoilib.me/";

        public override string SiteName => "YaoiLib.me";
    }
}