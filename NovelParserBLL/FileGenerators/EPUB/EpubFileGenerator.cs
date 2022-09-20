using NovelParserBLL.Models;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

namespace NovelParserBLL.FileGenerators.EPUB
{
    internal class EpubFileGenerator : IFileGenerator
    {
        public void Generate(string file, Novel novel, string translationTeam)
        {
            List<Chapter>? chapters;
            if (novel.ChaptersByTranslationTeam == null || !novel.ChaptersByTranslationTeam.TryGetValue(translationTeam, out chapters)) return;

            Document doc = new Document();
            doc.BuiltinDocumentProperties.Author = novel.Author;
            doc.BuiltinDocumentProperties.CreateDate = DateTime.Now;
            doc.BuiltinDocumentProperties.Title = novel.NameRus;

            foreach (var item in chapters)
            {
                Section section = doc.AddSection();
                section.PageSetup.Margins.All = 40f;
                Paragraph titleParagraph = section.AddParagraph();
                titleParagraph.AppendText(string.IsNullOrEmpty(item.Name) ? $"Глава {item.Number}" : item.Name);
                titleParagraph.Format.AfterSpacing = 10;
                titleParagraph.Format.HorizontalAlignment = HorizontalAlignment.Center;
                titleParagraph.ApplyStyle(BuiltinStyle.Heading1);

                Paragraph bodyParagraph_1 = section.AddParagraph();
                bodyParagraph_1.AppendHTML(item.Content ?? "");
                bodyParagraph_1.Format.HorizontalAlignment = HorizontalAlignment.Justify;
                bodyParagraph_1.Format.FirstLineIndent = 30;
                bodyParagraph_1.Format.AfterSpacing = 10;
            }

            if (novel.Cover != null)
            {
                DocPicture cover = new DocPicture(doc);
                cover.LoadImage(novel.Cover);
                doc.SaveToEpub(file, cover);
            }
            else
            {
                doc.SaveToFile(file, FileFormat.EPub);
            }
        }
    }
}
