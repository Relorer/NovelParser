using OpenQA.Selenium.Chrome;
using Sayaka.Common;
using System.Drawing;

namespace NovelParserBLL.Utilities
{
    internal static class ChromeDriverHelper
    {
        public static readonly string DownloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");

        private static readonly string userDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\Google\\Chrome\\User Data\\NovelParser");

        private static ChromeOptions GetChromeDriverOptions()
        {
            ChromeOptions? chromeOptions = new ChromeOptions();
            Directory.CreateDirectory(DownloadPath);

            chromeOptions.AddArgument($"--user-agent={ProviderFakeUserAgent.Random}");
            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddExcludedArgument("enable-automation");
            chromeOptions.AddArgument("--allow-file-access-from-files");
            chromeOptions.AddArgument("--enable-file-cookies");
#if (RELEASE)
            chromeOptions.AddArgument("--window-size=1,1");
            chromeOptions.AddArgument("--window-position=-32000,-32000");
#else
            chromeOptions.AddArgument("--window-size=1000,1000");
            chromeOptions.AddArgument("--window-position=100,100");
#endif

            chromeOptions.AddArguments(@$"user-data-dir={userDataPath}");
            chromeOptions.AddUserProfilePreference("download.default_directory", DownloadPath);

            return chromeOptions;
        }

        public static ChromeDriver StartChrome()
        {
            var driver = new ChromeDriver(GetChromeDriverOptions());
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            return driver;
        }

        public static string GetDownloadedPath(string fileName)
        {
            return Path.Combine(DownloadPath, fileName);
        }
    }
}
