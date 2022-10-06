using Microsoft.Win32;
using NovelParserBLL.FileGenerators;

namespace NovelParserWPF.DialogWindows
{
    internal static class FileDialogHelper
    {
        public static string GetSaveFilePath(string file, FileFormat fileFormat)
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

        private static string GetFileFilterByFilterFormat(FileFormat fileFormat) => fileFormat switch
        {
            FileFormat.EPUB => "EPUB file|*.epub",
            FileFormat.PDF => "PDF file|*.pdf",
            _ => throw new System.NotImplementedException(),
        };
    }
}