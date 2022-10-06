using NovelParserBLL.FileGenerators.PDF;
using NovelParserBLL.Models;

var pdfGenerator = new PdfFileGenerator();

var testGroupName = "test group";

var novel = new Novel()
{
    Name = "Test novel name",
    Description = "Test novel description",
    Author = "no author",
    URL = "none",
    Cover = GetTestCover(),
    ChaptersByGroup = new Dictionary<string, SortedList<int, Chapter>>()
    {
        {
            testGroupName,
            new SortedList<int, Chapter>()
            {
                {1, GetTestChapter() },
            }
        }
    }
};

var pdfGeneratorParams = new PDFGenerationParams("", novel, testGroupName, "all", PDFType.LongPage);

pdfGenerator.ShowInPreviewer(pdfGeneratorParams);

Chapter GetTestChapter()
{
    var chapter = new Chapter()
    {
        Content = ""
    };

    return chapter;
}

ImageInfo GetTestCover()
{
    return GetTestImage("coverTestImage.png");
}

ImageInfo GetTestShortImage()
{
    return GetTestImage("shortTestImage.png");
}

ImageInfo GetTestLongImage()
{
    return GetTestImage("longTestImage.png");
}

ImageInfo GetTestImage(string fileName)
{
    return new ImageInfo($"Assets/{fileName}", fileName, "");
}