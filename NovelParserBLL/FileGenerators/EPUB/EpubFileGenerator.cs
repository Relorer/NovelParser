using EpubSharp;

namespace NovelParserBLL.FileGenerators.EPUB
{
    internal class EpubFileGenerator : IFileGenerator<EPUBGenerationParams>
    {
        public Task Generate(EPUBGenerationParams generationParams)
        {
            return Task.Run(() =>
            {
                var novel = generationParams.Novel;

                EpubWriter writer = new EpubWriter();

                writer.AddAuthor(novel.Author);

                if (novel.Cover?.TryGetByteArray(out byte[]? cover) ?? false)
                {
                    writer.SetCover(cover, ImageFormat.Png);
                }

                writer.SetTitle(novel.Name);

                foreach (var chapter in generationParams.Chapters)
                {
                    var title = string.IsNullOrEmpty(chapter.Value.Name) ? $"Глава {chapter.Value.Number}" : chapter.Value.Name;
                    var content = $"<h2>{title}</h2>" + chapter.Value.Content;
                    writer.AddChapter(title, content);
                    foreach (var item in chapter.Value.Images)
                    {
                        if (item.TryGetByteArray(out byte[]? img))
                        {
                            writer.AddFile(item.Name, img, EpubSharp.Format.EpubContentType.ImagePng);
                        }
                    }
                }
                writer.Write(generationParams.FilePath);
            });
        }
    }
}