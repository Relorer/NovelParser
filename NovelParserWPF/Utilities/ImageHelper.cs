using System.IO;
using System.Windows.Media.Imaging;

namespace NovelParserWPF.Utilities
{
    internal class ImageHelper
    {
        public static BitmapImage BitmapImageFromBuffer(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
    }
}