using Microsoft.Win32;
using NovelParserBLL.FileGenerators;

namespace NovelParserWPF.Utilities
{
    internal static class FileHelper
    {
        public static string GetSaveFilePath(string file, FileFormatForGenerator fileFormat)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = GetFileFilterByFilterFormat(fileFormat);
            saveFileDialog.Title = $"Save an {fileFormat} File";
            saveFileDialog.FileName = file;
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                file = saveFileDialog.FileName;
            }

            return file;
        }

        private static string GetFileFilterByFilterFormat(FileFormatForGenerator fileFormat) => fileFormat switch
        {
            FileFormatForGenerator.EPUB => "EPUB file|*.epub",
            FileFormatForGenerator.PDF => "PDF file|*.pdf",
        };
    }
}