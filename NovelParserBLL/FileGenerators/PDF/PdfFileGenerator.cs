using NovelParserBLL.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal class PdfFileGenerator : IFileGenerator
    {
        public Task Generate(string file, Novel novel, string group, string pattern)
        {
            return Task.Run(() =>
            {
                PdfDocument fullPdf = new PdfDocument();

                var chaptersWithCover = new SortedList<int, Chapter>(novel[group, pattern]);
                chaptersWithCover.Add(-1, new Chapter() { 
                    Name = "Cover", 
                    Content = "<img src=\"cover\"/>", 
                    Images = new Dictionary<string, byte[]>() { 
                        { "cover", novel.Cover! } 
                    } 
                });

                foreach (var chapter in chaptersWithCover)
                {
                    var title = string.IsNullOrEmpty(chapter.Value.Name) ? $"Глава {chapter.Value.Number}" : chapter.Value.Name;
                    var content = $"<h2>{title}</h2>" + chapter.Value.Content;

                    PdfDocument pdf = PdfGenerator.GeneratePdf(content.ToString(), PageSize.A4, imageLoad: (_, e) =>
                    {
                        var imgName = e.Src;
                        using MemoryStream stream = new MemoryStream(chapter.Value.Images[imgName]);

                        Image fullsizeImage = Image.FromStream(stream);

                        Image newImage = ResizeImage(fullsizeImage, new Size(550, 500));

                        using MemoryStream result = new MemoryStream();

                        newImage.Save(result, System.Drawing.Imaging.ImageFormat.Png);

                        XImage img = XImage.FromStream(result);
                        e.Callback(img);
                    });

                    using (var tempMemoryStream = new MemoryStream())
                    {
                        pdf.Save(tempMemoryStream, false);
                        var openedDoc = PdfReader.Open(tempMemoryStream, PdfDocumentOpenMode.Import);
                        foreach (PdfPage page in openedDoc.Pages)
                        {
                            fullPdf.AddPage(page);
                        }
                    }
                }

                fullPdf.Save(file);
            });
        }

        private static Image ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercentW = size.Width / (float)sourceWidth;
            float nPercentH = size.Height / (float)sourceHeight;

            float nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap resultBitmap = new Bitmap(destWidth, destHeight);
            Graphics resultGraphics = Graphics.FromImage(resultBitmap);
            resultGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            resultGraphics.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            resultGraphics.Dispose();

            return resultBitmap;
        }
    }
}
