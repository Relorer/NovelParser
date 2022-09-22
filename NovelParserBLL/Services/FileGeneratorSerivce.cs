using NovelParserBLL.FileGenerators;
using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.FileGenerators.PDF;
using NovelParserBLL.Models;
using NovelParserBLL.Utilities;

namespace NovelParserBLL.Services
{
    public enum FileFormat
    {
        EPUB,
        PDF,
    }

    public static class FileGeneratorService
    {
        private static Dictionary<FileFormat, IFileGenerator> fileGenerators = new Dictionary<FileFormat, IFileGenerator>()
        {
            {FileFormat.EPUB, new EpubFileGenerator() },
            {FileFormat.PDF, new PdfFileGenerator() }
        };

        public static Task Generate(string file, FileFormat format, Novel novel, SortedList<int, Chapter> chapters)
        {
            return fileGenerators[format].Generate(FileHelper.AddFileExtension(file, format), novel, chapters);
        }
    }
}
