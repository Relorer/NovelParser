using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NovelParserBLL.Models;

namespace NovelParserBLL.Utilities
{
    internal class HTMLHelper
    {
        protected readonly HtmlParser parser = new HtmlParser();

        public async Task<(string, List<ImageInfo>)> LoadImagesForHTML(string html, Func<IElement, Task<ImageInfo>> downloadImageByHTMLTag)
        {
            var document = parser.ParseDocument(html);

            var images = new List<ImageInfo>();

            foreach (var item in document.QuerySelectorAll("img"))
            {
                var img = await downloadImageByHTMLTag(item);
                images.Add(img);
                item.SetAttribute("src", img.Name);
            }

            return (document.Body?.InnerHtml ?? "", images);
        }

        public string RemoveImagesFromHTML(string html)
        {
            var document = parser.ParseDocument(html);

            foreach (var item in document.QuerySelectorAll("img"))
            {
                item.Remove();
            }

            return document.Body?.InnerHtml ?? "";
        }
    }
}