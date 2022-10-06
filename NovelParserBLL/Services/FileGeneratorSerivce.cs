using NovelParserBLL.FileGenerators;
using NovelParserBLL.FileGenerators.EPUB;
using NovelParserBLL.FileGenerators.PDF;

namespace NovelParserBLL.Services
{
    public class FileGeneratorService
    {
        private PdfFileGenerator pdfGenerator = new PdfFileGenerator();
        private EpubFileGenerator epubFileGenerator = new EpubFileGenerator();

        public Task Generate(GenerationParams generationParams)
        {
            switch (generationParams)
            {
                case PDFGenerationParams:
                    return pdfGenerator.Generate((PDFGenerationParams)generationParams);

                case EPUBGenerationParams:
                    return epubFileGenerator.Generate((EPUBGenerationParams)generationParams);

                default:
                    throw new ArgumentException(nameof(generationParams));
            }
        }
    }
}