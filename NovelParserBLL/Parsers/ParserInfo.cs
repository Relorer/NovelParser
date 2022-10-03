namespace NovelParserBLL.Parsers
{
    public class ParserInfo
    {
        public string SiteDomen { get; }
        public string AuthPage { get; }
        public string SiteName { get; }

        public ParserInfo(string siteDomen, string siteName, string authPage)
        {
            SiteDomen = siteDomen;
            SiteName = siteName;
            AuthPage = authPage;
        }
    }
}