using NovelParserBLL.Services;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Utilities
{
    public static class FileHelper
    {
        public static string AddFileExtension(string file, FileFormat format)
        {
            return file + (file.ToUpper().EndsWith(format.ToString()) ? "" : $".{format.ToString().ToLower()}");
        }

        public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar = "")
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, replaceChar);
        }
    }
}
