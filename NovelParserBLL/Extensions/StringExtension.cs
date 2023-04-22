using System.Text.RegularExpressions;

namespace NovelParserBLL.Extensions
{
    internal static class StringExtension
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }

        /// <summary>
        /// Deletes substring between markers (include markers) from string 
        /// Markers must be unique
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startMarker"></param>
        /// <param name="endMarker"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string DeleteSubstring(this string str, string startMarker, string endMarker)
        {
            var startIndex = str.IndexOf(startMarker, 0, StringComparison.OrdinalIgnoreCase);
            var endIndex = str.IndexOf(endMarker, 0, StringComparison.OrdinalIgnoreCase);
        
            if (startIndex == -1 || endIndex == -1)
                throw new ArgumentException("Could not find markers.");

            if (startIndex >= endIndex)
                throw new ArgumentException("Markers positioning error.");

            var substringLen = endIndex - startIndex + endMarker.Length;
            return str.Remove(startIndex, substringLen);
        }

        public static string DeleteSubstring(this string str, string subString)
        {
            return str.Replace(subString, "");
        }
    }
}