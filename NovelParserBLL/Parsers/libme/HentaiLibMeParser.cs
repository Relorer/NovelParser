using NovelParserBLL.Services;
using NovelParserBLL.Services.Interfaces;

namespace NovelParserBLL.Parsers.LibMe;

internal class HentaiLibMeParser : ComicsLibMeParser
{
    public HentaiLibMeParser(SetProgress setProgress, IWebClient webClient) 
        : base(setProgress, webClient)
    {
    }

    public override string SiteName => "HentaiLib.me";

    public override string SiteDomain => "https://hentailib.me/";

    protected override List<string> Servers => new()
    {
        "https://img2.hentailib.org",
        "https://img3.hentailib.org",
        "https://img4.hentailib.org",
    };
}