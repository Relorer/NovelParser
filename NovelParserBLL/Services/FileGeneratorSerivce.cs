using NovelParserBLL.FileGenerators;
using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.FileGenerators.PDF;

namespace NovelParserBLL.Services
{
    public class FileGeneratorService
    {
        private PDFFileGenerator pdfGenerator = new();
        private EpubFileGenerator epubFileGenerator = new();

        public Task Generate(GenerationParams generationParams)
        {
            return generationParams switch
            {
                PDFGenerationParams @params => pdfGenerator.Generate(@params),
                EPUBGenerationParams @params => epubFileGenerator.Generate(@params),
                _ => throw new ArgumentException(nameof(generationParams))
            };
        }
    }
}