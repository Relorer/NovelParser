using NovelParserBLL.Models;
using EpubSharp;

namespace NovelParserBLL.FileGenerators.EPUB
{
    public class EpubFileGenerator : IFileGenerator
    {
        public Task Generate(string file, Novel novel, SortedList<int, Chapter> chapters)
        {
            return Task.Run(() =>
            {
                EpubWriter writer = new EpubWriter();

                writer.AddAuthor(novel.Author);
                writer.SetCover(novel.Cover, ImageFormat.Png);
                writer.SetTitle(novel.NameRus);

                foreach (var chapter in chapters)
                {
                    var title = string.IsNullOrEmpty(chapter.Value.Name) ? $"Глава {chapter.Value.Number}" : chapter.Value.Name;
                    var content = $"<h2>{title}<h2>" + chapter.Value.Content;
                    writer.AddChapter(title, content);
                    foreach (var item in chapter.Value.Images)
                    {
                        writer.AddFile(item.Key, item.Value, EpubSharp.Format.EpubContentType.ImagePng);
                    }
                }
                writer.Write(file);
            });

        }
    }
}
