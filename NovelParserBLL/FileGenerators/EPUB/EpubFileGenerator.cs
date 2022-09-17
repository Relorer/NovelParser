using NovelParserBLL.Models;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

namespace NovelParserBLL.FileGenerators.EPUB
{
    internal class EpubFileGenerator : IFileGenerator
    {
        public void Generate(string file, Novel novel)
        {
            if (novel.Chapters == null) return;

            Document doc = new Document();

            foreach (var item in novel.Chapters)
            {
                Section section = doc.AddSection();
                section.PageSetup.Margins.All = 40f;
                Paragraph titleParagraph = section.AddParagraph();
                titleParagraph.AppendText(string.IsNullOrEmpty(item.Name) ? $"Глава {item.Number}" : item.Name);
                titleParagraph.Format.AfterSpacing = 10;
                titleParagraph.Format.HorizontalAlignment = HorizontalAlignment.Center;

                Paragraph bodyParagraph_1 = section.AddParagraph();
                bodyParagraph_1.AppendHTML(item.Content ?? "");
                bodyParagraph_1.Format.HorizontalAlignment = HorizontalAlignment.Justify;
                bodyParagraph_1.Format.FirstLineIndent = 30;
                bodyParagraph_1.Format.AfterSpacing = 10;
            }

            

            if (novel.Chapters != null)
            {
                DocPicture cover = new DocPicture(doc);
                cover.LoadImage(File.ReadAllBytes(novel.CoverPath!));
                doc.SaveToEpub(file, cover);
            }
            else
            {
                doc.SaveToFile(file, FileFormat.EPub);
            }
        }
    }
}
