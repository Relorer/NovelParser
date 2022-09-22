using NovelParserBLL.Extensions;
using NovelParserBLL.Utilities;
using OpenQA.Selenium.Chrome;
using Sayaka.Common;
using System.Configuration;
using System.Drawing;

namespace NovelParserBLL.Services
{
    public static class ChromeDriverHelper
    {
        public static readonly string DownloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");

        private static readonly string userDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\Google\\Chrome\\User Data\\NovelParser");

        private static ChromeOptions GetChromeDriverOptions(bool visible = false)
        {
            ChromeOptions? chromeOptions = new ChromeOptions();
            Directory.CreateDirectory(DownloadPath);

            chromeOptions.AddArgument($"--user-agent={ProviderFakeUserAgent.Random}");
            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddExcludedArgument("enable-automation");
            chromeOptions.AddArgument("--allow-file-access-from-files");

#if (RELEASE)
            if (visible)
            {
                chromeOptions.AddArgument("--window-size=1000,1000");
                chromeOptions.AddArgument("--window-position=100,100");
            }
            else
            {
                chromeOptions.AddArgument("--window-size=1,1");
                chromeOptions.AddArgument("--window-position=-32000,-32000");
            }
#else
            chromeOptions.AddArgument("--window-size=1000,1000");
            chromeOptions.AddArgument("--window-position=100,100");
#endif

            if (bool.Parse(ConfigurationManager.AppSettings["UseCookies"] ?? "false"))
            {
                chromeOptions.AddArguments(@$"user-data-dir={userDataPath}");
                chromeOptions.AddArgument("--enable-file-cookies");
            }
            chromeOptions.AddUserProfilePreference("download.default_directory", DownloadPath);
            return chromeOptions;
        }

        public static ChromeDriver StartChrome(bool visible = false)
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(chromeDriverService, GetChromeDriverOptions(visible));
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            return driver;
        }

        public static string GetDownloadedPath(string fileName)
        {
            return Path.Combine(DownloadPath, fileName);
        }

        public static void ClearCookies()
        {
            if (Directory.Exists(userDataPath))
            {
                DirectoryHelper.Empty(userDataPath);
            }
        }

        public static ChromeDriver OpenPageWithAutoClose(string url)
        {
            var driver = StartChrome(true);
            driver.GoTo(url);

            Task.Run(async () =>
            {
                await Task.Delay(15 * 60_000);
                driver?.Close();
                driver?.Dispose();
            });

            return driver;
        }
    }
}
