/*
using Newtonsoft.Json;
using NovelParserBLL.Properties;
using NovelParserBLL.Utilities;
using OpenQA.Selenium.Chrome;
using Sayaka.Common;
using System.Configuration;

namespace NovelParserBLL.Services.ChromeDriverHelper
{
    public static class ChromeDriverHelper
    {
        public static readonly string DownloadPath = Path.Combine(Directory.GetCurrentDirectory(), Resources.CacheFolder);

        private static readonly string userDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\Google\\Chrome\\User Data\\NovelParser");

        public static void ClearCookies()
        {
            if (Directory.Exists(userDataPath))
            {
                DirectoryHelper.Empty(userDataPath);
            }
        }

        public static ChromeDriver OpenPageWithAutoClose(string url, int time = 15 * 60_000)
        {
            var driver = StartChrome(true);
            driver.GoTo(url);

            Task.Run(async () =>
            {
                await Task.Delay(time);
                driver?.Close();
                driver?.Dispose();
            });

            return driver;
        }

        internal static async Task<ChromeDriver> TryLoadPage(string url, string checkChallengeRunningScript = "return false", string downloadFolder = "")
        {
            var timeSpan = new TimeSpan(0, 30, 0);
            var driver = StartChrome(false, downloadFolder, timeSpan);

            driver.GoTo(url);

            var check = () => JsonConvert.DeserializeObject<bool>((string)driver.ExecuteScript(checkChallengeRunningScript));
            var attempt = 1;
            while (check() && attempt < 4)
            {
                await Task.Delay(4000);

                if (!check()) break;
                driver.Dispose();

                driver = StartChrome(false, downloadFolder, timeSpan);
                driver.GoTo(url);

                await Task.Delay(attempt++ * 2000);
            }

            await Task.Delay(attempt * 1000);

            return driver;
        }

        private static ChromeOptions GetChromeDriverOptions(bool visible = false, string downloadFolder = "")
        {
            ChromeOptions? chromeOptions = new ChromeOptions();
            Directory.CreateDirectory(DownloadPath);

            chromeOptions.AddArgument($"--user-agent={ProviderFakeUserAgent.Random}");
            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddExcludedArgument("enable-automation");
            chromeOptions.AddArgument("--allow-file-access-from-files");

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

            if (bool.Parse(ConfigurationManager.AppSettings["UseCookies"] ?? "false"))
            {
                chromeOptions.AddArguments(@$"user-data-dir={userDataPath}");
                chromeOptions.AddArgument("--enable-file-cookies");
            }

            downloadFolder = string.IsNullOrEmpty(downloadFolder) ? DownloadPath : Path.GetFullPath(downloadFolder);
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadFolder);
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);

            return chromeOptions;
        }

        private static void GoTo(this ChromeDriver drive, string url)
        {
            drive.Navigate().GoToUrl(url);
        }

        private static ChromeDriver StartChrome(bool visible = false, string downloadFolder = "", TimeSpan? timeSpan = null)
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(chromeDriverService, GetChromeDriverOptions(visible, downloadFolder));
            if (timeSpan != null)
            {
                driver.Manage().Timeouts().ImplicitWait = timeSpan.Value;
            }
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            return driver;
        }
    }
}*/