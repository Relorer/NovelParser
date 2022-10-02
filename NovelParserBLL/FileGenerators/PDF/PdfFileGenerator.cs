using NovelParserBLL.Models;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal class PdfFileGenerator : IFileGenerator
    {
        public Task Generate(string file, Novel novel, string group, string pattern)
        {
            return Task.Run(() =>
            {
                //PdfDocument fullPdf = new PdfDocument();

                //var chaptersWithCover = new SortedList<int, Chapter>(novel[group, pattern]);
                //chaptersWithCover.Add(-1, new Chapter() {
                //    Name = "Cover",
                //    Content = "<img src=\"cover\"/>",
                //    Images = new Dictionary<string, byte[]>() {
                //        { "cover", novel.Cover! }
                //    }
                //});

                //foreach (var chapter in chaptersWithCover)
                //{
                //    var title = string.IsNullOrEmpty(chapter.Value.Name) ? $"Глава {chapter.Value.Number}" : chapter.Value.Name;
                //    var content = $"<h2>{title}</h2>" + chapter.Value.Content;

                //    PdfDocument pdf = PdfGenerator.GeneratePdf(content.ToString(), PageSize.A4, imageLoad: (_, e) =>
                //    {
                //        var imgName = e.Src;
                //        if (chapter.Value.Images[imgName] == null || chapter.Value.Images[imgName].Length == 0) return;

                //        Image image = Image.Load(chapter.Value.Images[imgName]);

                //        using MemoryStream result = new MemoryStream();

                //        image.Mutate(i => i.Resize(550, 550 / image.Width * image.Height));
                //        image.SaveAsPng(result);

                //        XImage img = XImage.FromStream(result);
                //        e.Callback(img);
                //    });

                //    using (var tempMemoryStream = new MemoryStream())
                //    {
                //        pdf.Save(tempMemoryStream, false);
                //        var openedDoc = PdfReader.Open(tempMemoryStream, PdfDocumentOpenMode.Import);
                //        foreach (PdfPage page in openedDoc.Pages)
                //        {
                //            fullPdf.AddPage(page);
                //        }
                //    }
                //}

                //fullPdf.Save(file);
            });
        }
    }
}