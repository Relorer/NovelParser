using NovelParserBLL.Services;

namespace NovelParserBLL.Parsers.libme
{
    internal class YaoiLibMeParser : ComixLibMeParser
    {
        public YaoiLibMeParser(SetProgress setProgress) : base(setProgress)
        {
        }

        protected override string domen => "https://yaoilib.me/";
    }
}