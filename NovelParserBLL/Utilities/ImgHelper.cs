using System.Drawing;

namespace NovelParserBLL.Utilities
{
    internal class ImgHelper
    {
        public async static Task<string> GetImageAsBase64Url(string url)
        {
            using var handler = new HttpClientHandler();
            using var client = new HttpClient(handler);

            var bytes = await client.GetByteArrayAsync(url);
            return "image/jpeg;base64," + Convert.ToBase64String(bytes);
        }

        public static string ImageFileToBase64(string path)
        {
            byte[] imageArray = File.ReadAllBytes(path);
            return "image/jpeg;base64," + Convert.ToBase64String(imageArray);
        }

    }
}
