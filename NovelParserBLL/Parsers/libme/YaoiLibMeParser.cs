using NovelParserBLL.Services;
using NovelParserBLL.Services.Interfaces;

namespace NovelParserBLL.Parsers.LibMe;

internal class YaoiLibMeParser : ComicsLibMeParser
{
    public YaoiLibMeParser(SetProgress setProgress, IWebClient webClient) 
        : base(setProgress, webClient)
    {
    }

    public override string SiteDomain => "https://yaoilib.me/";

    public override string SiteName => "YaoiLib.me";
}