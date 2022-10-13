using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;

namespace HTMLQuestPDF.Components.Tags
{
    internal class ImgComponent : BaseHTMLComponent
    {
        private readonly Func<string, string> getImagePath;
        private readonly PageSize containerSize;

        public ImgComponent(Func<string, string> getImagePath, PageSize containerSize, HtmlNode node) : base(node)
        {
            this.getImagePath = getImagePath;
            this.containerSize = containerSize;
        }

        public void Compose(IContainer container)
        {
            var src = node.GetAttributeValue("src", "");
            var fullPath = getImagePath(src);

            var item = container.AlignCenter();

            if (File.Exists(fullPath))
            {
                item.Element(e =>
                {
                    using Image image = Image.Load(fullPath);
                    var requiredHeight = image.Height * (containerSize.Width / image.Width);
                    return requiredHeight > containerSize.Height ? e.MinHeight(containerSize.Height) : e;
                })
                    .Element(e =>
                    {
                        return true ? e : e.Hyperlink("");
                    })
                    .Image(fullPath, ImageScaling.FitArea);
            }
            else
            {
                item.Image(Placeholders.Image(200, 100), ImageScaling.FitArea);
            }
        }
    }
}