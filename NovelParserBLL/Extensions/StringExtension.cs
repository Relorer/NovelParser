using System.Text.RegularExpressions;

namespace NovelParserBLL.Extensions
{
    internal static class StringExtension
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }
    }
}