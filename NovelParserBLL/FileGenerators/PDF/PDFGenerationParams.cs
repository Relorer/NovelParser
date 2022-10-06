using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators.PDF
{
    public enum PDFType
    {
        FixPageSize,
        LongPage
    }

    public class PDFGenerationParams : GenerationParams
    {
        public PDFGenerationParams(FileFormat fileFormat, string filePath, Novel novel, string group, string pattern, PDFType pdfType) : base(fileFormat, filePath, novel, group, pattern)
        {
            this.PDFType = pdfType;
        }

        public PDFType PDFType { get; init; }
    }
}