using OpenQA.Selenium.Chrome;

namespace NovelParserBLL.Extensions
{
    public static class ChromeDriverExtension
    {
        public static void GoTo(this ChromeDriver drive, string url)
        {
            drive.Navigate().GoToUrl(url);
        }
    }
}
