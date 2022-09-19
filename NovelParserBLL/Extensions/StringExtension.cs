using System.Text.RegularExpressions;

namespace NovelParserBLL.Extensions
{
    public static class StringExtension
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", String.Empty);
        }
    }
}
