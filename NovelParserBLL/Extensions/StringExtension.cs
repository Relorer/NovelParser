using System.Text.RegularExpressions;

namespace NovelParserBLL.Extensions
{
    internal static class StringExtension
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }

        public static string Clean(this string value)
        {
            return Regex.Replace(value, @"[ \r\n]+", " ").Trim();
        }
    }
}