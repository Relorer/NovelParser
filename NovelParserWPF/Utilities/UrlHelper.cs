using System.Diagnostics;

namespace NovelParserWPF.Utilities
{
    internal static class UrlHelper
    {
        public static void OpenUrlInDefaultBrowser(string url)
        {
            Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
        }
    }
}
