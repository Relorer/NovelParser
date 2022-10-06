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
        public PDFGenerationParams(string filePath, Novel novel, string group, string pattern, PDFType pdfType) : base(filePath, novel, group, pattern)
        {
            this.PDFType = pdfType;
        }

        public PDFType PDFType { get; init; }
    }
}