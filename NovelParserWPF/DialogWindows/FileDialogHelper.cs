using System.IO;
using Microsoft.Win32;
using NovelParserBLL.FileGenerators;

namespace NovelParserWPF.DialogWindows;

internal static class FileDialogHelper
{
    public static string GetSaveFilePath(string file, FileFormat fileFormat)
    {
        file = file.Replace(" ", "_");

        var saveFileDialog = new SaveFileDialog
        {
            Filter = GetFileFilterByFilterFormat(fileFormat),
            Title = $"Save an {fileFormat} File",
            FileName = Path.GetFileName(file),
            InitialDirectory = Path.GetDirectoryName(file),
        };

        saveFileDialog.ShowDialog();
        var filename = saveFileDialog.FileName;

        if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            return file;

        if (!string.IsNullOrWhiteSpace(Path.GetExtension(filename)))
            return filename;

        filename += fileFormat switch
        {
            FileFormat.EPUB => ".epub",
            FileFormat.PDF => ".pdf",
            _ => "",
        };

        return filename;
    }

    private static string GetFileFilterByFilterFormat(FileFormat fileFormat) 
        => fileFormat switch
    {
        FileFormat.EPUB => "EPUB file|*.epub",
        FileFormat.PDF => "PDF file|*.pdf",
        _ => throw new System.NotImplementedException(),
    };
}