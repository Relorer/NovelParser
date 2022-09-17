using OpenQA.Selenium.Chrome;
using Sayaka.Common;

namespace NovelParserBLL.Utilities
{
    internal static class ChromeDriverHelper
    {
        public static ChromeDriver StartChrome()
        {
            var chromeOptions = new ChromeOptions();

            var userAgent = ProviderFakeUserAgent.Random;
            Console.WriteLine($"random {userAgent}");
            chromeOptions.AddArgument($"--user-agent={userAgent}");

#if (!DEBUG)
                chromeOptions.AddArguments("headless");
#endif
            Directory.CreateDirectory("Downloads");

            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddExcludedArgument("enable-automation");
            chromeOptions.AddUserProfilePreference("download.default_directory", Path.Combine(Directory.GetCurrentDirectory(), "Downloads"));
            chromeOptions.AddArgument("--allow-file-access-from-files");
            chromeOptions.AddArgument("--enable-file-cookies");

            chromeOptions.AddAdditionalChromeOption("useAutomationExtension", false);
            
            chromeOptions.AddArguments(@$"user-data-dir={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Local\\Google\\Chrome\\User Data\\NovelParser")}");
            var driver = new ChromeDriver(chromeOptions);
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

            return driver;
        }

    }
}
