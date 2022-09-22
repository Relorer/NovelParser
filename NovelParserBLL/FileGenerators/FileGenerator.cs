using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.FileGenerators.PDF;
using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators
{
    public static class FileGenerator
    {
        private static List<IFileGenerator> fileGenerators = new List<IFileGenerator>()
        {
            new EpubFileGenerator(),
            new PdfFileGenerator()
        };

        public static Task Generate(string file, FileFormatForGenerator fileFormat, Novel novel, SortedList<int, Chapter> chapters)
        {
            foreach (var item in fileGenerators)
            {
                if (fileFormat == item.SupportedFileFormat)
                {
                    file += file.ToUpper().EndsWith(fileFormat.ToString()) ? $".{fileFormat.ToString().ToLower()}" : "";
                    return item.Generate($"{file}.{fileFormat}", novel, chapters);
                }
            }
            throw new ArgumentException(nameof(file));
        }
    }
}
