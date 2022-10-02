using NovelParserBLL.Models;
using NovelParserBLL.Services;
using NovelParserBLL.Services.ChromeDriverHelper;
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
            var result = r.Replace(filename, replaceChar);
            return result.Substring(0, Math.Min(result.Length, 100));
        }

        public static ImageInfo UpdateImageInfo(ImageInfo imageInfo, Novel novel)
        {
            if (imageInfo != null && !imageInfo.Exists && !string.IsNullOrEmpty(imageInfo.NameFromURL))
            {
                var downloadFolder = Path.Combine(ChromeDriverHelper.DownloadPath, novel.DownloadFolderName);
                var downloadedImagePath = Path.Combine(downloadFolder, imageInfo.NameFromURL);

                if (File.Exists(downloadedImagePath))
                {
                    var result = new ImageInfo(downloadFolder, imageInfo.URL);
                    File.Move(downloadedImagePath, result.FullPath);
                    return result;
                }
            }

            return imageInfo!;
        }
    }
}