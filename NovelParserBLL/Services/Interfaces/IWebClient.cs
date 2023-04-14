namespace NovelParserBLL.Services.Interfaces;

public interface IWebClient
{
    public const string DefaultAgent =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:101.0) Gecko/20100101 Firefox/101.0";

    Task<string> GetStringContentAsync(string url, string agent = DefaultAgent, CancellationToken token = default);
    Task<byte[]> GetBinaryContentAsync(string url, string agent = DefaultAgent, CancellationToken token = default);
}