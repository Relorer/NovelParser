namespace NovelParserBLL.Parsers
{
    public class ParserInfo
    {
        public string SiteDomain { get; }
        public string AuthPage { get; }
        public string SiteName { get; }
        public ParserInfo(string siteDomain, string siteName, string authPage)
        {
            SiteDomain = siteDomain;
            SiteName = siteName;
            AuthPage = authPage;
        }
    }
}