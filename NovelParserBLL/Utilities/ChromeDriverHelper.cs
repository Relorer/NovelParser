using OpenQA.Selenium.Chrome;
using Sayaka.Common;

namespace NovelParserBLL.Utilities
{
    internal static class ChromeDriverHelper
    {
        public static readonly string DownloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");

        private static ChromeOptions? chromeOptions = new ChromeOptions();
        private static readonly string userDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\Google\\Chrome\\User Data\\NovelParser");

        static ChromeDriverHelper()
        {
            Directory.CreateDirectory(DownloadPath);
            Console.WriteLine(ProviderFakeUserAgent.Random);
            chromeOptions.AddArgument($"--user-agent={ProviderFakeUserAgent.Random}");
            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddExcludedArgument("enable-automation");
            chromeOptions.AddArgument("--allow-file-access-from-files");
            chromeOptions.AddArgument("--enable-file-cookies");
#if (RELEASE)
            chromeOptions.AddArgument("--window-size=1,1");
#endif
            chromeOptions.AddArguments(@$"user-data-dir={userDataPath}");
            chromeOptions.AddUserProfilePreference("download.default_directory", DownloadPath);
        }

        public static ChromeDriver StartChrome()
        {
            var driver = new ChromeDriver(chromeOptions);
#if (RELEASE)
            driver.Manage().Window.Position = new System.Drawing.Point(-4000, 0);
#endif
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            return driver;
        }

        public static string GetDownloadedPath(string fileName)
        {
            return Path.Combine(DownloadPath, fileName);
        }
    }
}
