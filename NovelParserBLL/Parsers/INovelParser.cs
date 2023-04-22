using NovelParserBLL.Models;

namespace NovelParserBLL.Parsers;

internal interface INovelParser
{
    public ParserInfo ParserInfo { get; }
    Task LoadChapters(Novel novel, string group, string pattern, bool includeImages, CancellationToken token);
    Task<Novel> ParseCommonInfo(Novel novel, CancellationToken token);
    bool ValidateUrl(string url);
    string PrepareUrl(string url);
}