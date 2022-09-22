using NovelParserBLL.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace NovelParserBLL.FileGenerators.PDF
{
    internal class PdfFileGenerator : IFileGenerator
    {
        public FileFormatForGenerator SupportedFileFormat => FileFormatForGenerator.PDF;

        public Task Generate(string file, Novel novel, SortedList<int, Chapter> chapters)
        {
            return Task.Run(() =>
            {
                var size = chapters.Select(ch => ch.Value.Content.Length).Aggregate((c1, c2) => c1 + c2);
                var fullHtml = new StringBuilder(size);

                foreach (var chapter in chapters)
                {
                    var title = string.IsNullOrEmpty(chapter.Value.Name) ? $"Глава {chapter.Value.Number}" : chapter.Value.Name;
                    fullHtml.Append($"<h2>{title}</h2>" + chapter.Value.Content);
                }

                var images = new Dictionary<string, byte[]>(chapters.SelectMany(ch => ch.Value.Images));

                PdfDocument pdf = PdfGenerator.GeneratePdf(fullHtml.ToString(), PageSize.A4, imageLoad: (_, e) =>
                {
                    var imgName = e.Src;
                    using MemoryStream stream = new MemoryStream(images[imgName]);

                    Image fullsizeImage = Image.FromStream(stream);

                    Image newImage = resizeImage(fullsizeImage, new Size(550, 500));

                    using MemoryStream result = new MemoryStream();

                    newImage.Save(result, System.Drawing.Imaging.ImageFormat.Png);

                    XImage img = XImage.FromStream(result);
                    e.Callback(img);
                });

                pdf.Save(file);
            });
        }

        private static Image resizeImage(Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            //Calulate  width with new desired size  
            float nPercentW = size.Width / (float)sourceWidth;
            //Calculate height with new desired size  
            float nPercentH = size.Height / (float)sourceHeight;
            float nPercent;
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return b;
        }
    }
}
