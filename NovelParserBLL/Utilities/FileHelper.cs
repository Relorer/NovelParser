using NovelParserBLL.Models;
using System.Text.RegularExpressions;

namespace NovelParserBLL.Utilities;

public static class FileHelper
{
    public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar = "")
    {
        var regexSearch = GetInvalidCharacters();
        var r = new Regex($"[{Regex.Escape(regexSearch)}]");
        var result = r.Replace(filename, replaceChar);
        return result[..Math.Min(result.Length, 100)];
    }

    public static ImageInfo UpdateImageInfo(ImageInfo? imageInfo, string downloadFolder)
    {
        if (imageInfo is not { Exists: false } || string.IsNullOrEmpty(imageInfo.NameFromURL)) 
            return imageInfo!;

        var downloadedImagePath = Path.Combine(downloadFolder, imageInfo.NameFromURL);

        if (!ExistsRelatively(downloadedImagePath)) 
            return imageInfo;
        
        var result =
            !string.IsNullOrEmpty(imageInfo.Name) && !string.IsNullOrEmpty(imageInfo.FullPath) ?
                new ImageInfo(imageInfo.FullPath, imageInfo.Name, imageInfo.URL) :
                new ImageInfo(downloadFolder, imageInfo.URL);

        if (!File.Exists(result.FullPath))
        {
            File.Move(downloadedImagePath, result.FullPath);
        }
        return result;

    }

    private static bool ExistsRelatively(string path)
    {
        return File.Exists(Path.GetFullPath(path));
    }

    private static string GetInvalidCharacters()
    {
        return new string(Path.GetInvalidFileNameChars()
            .Concat(Path.GetInvalidPathChars())
            .ToArray());
    }
}