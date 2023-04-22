using NovelParserBLL.Services;
using NovelParserBLL.Services.Interfaces;

namespace NovelParserBLL.Parsers.LibMe;

internal class MangaLibMeParser : ComicsLibMeParser
{
    public MangaLibMeParser(SetProgress setProgress, IWebClient webClient) 
        : base(setProgress, webClient)
    {
    }

    public override string SiteDomain => "https://mangalib.me/";

    public override string SiteName => "MangaLib.me";
}