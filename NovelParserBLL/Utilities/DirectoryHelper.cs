
namespace NovelParserBLL.Utilities
{
    public class DirectoryHelper
    {
        public static void Empty(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            foreach (FileInfo file in dir.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in dir.GetDirectories()) subDirectory.Delete(true);
        }
    }
}