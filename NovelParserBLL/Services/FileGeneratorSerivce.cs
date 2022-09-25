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

    public class FileGeneratorService
    {
        private Dictionary<FileFormat, IFileGenerator> fileGenerators = new Dictionary<FileFormat, IFileGenerator>()
        {
            {FileFormat.EPUB, new EpubFileGenerator() },
            {FileFormat.PDF, new PdfFileGenerator() }
        };

        public Task Generate(string file, FileFormat format, Novel novel, string group, string pattern)
        {
            return fileGenerators[format].Generate(FileHelper.AddFileExtension(file, format), novel, group, pattern);
        }
    }
}