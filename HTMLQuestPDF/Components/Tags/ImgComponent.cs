using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using HTMLQuestPDF.Models;
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

        public ImgComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
            this.getImagePath = args.GetImagePath;
            this.containerSize = args.ContainerSize;
        }

        protected override void ComposeSingle(IContainer container)
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
                    .Image(fullPath, ImageScaling.FitArea);
            }
            else
            {
                item.Image(Placeholders.Image(200, 100), ImageScaling.FitArea);
            }
        }

    }
}