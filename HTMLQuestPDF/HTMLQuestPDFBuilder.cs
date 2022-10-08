using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using System.Xml.Linq;

namespace HTMLQuestPDF
{
    internal class HTMLQuestPDFBuilder
    {
        private delegate void TextSpan(TextSpanDescriptor textSpan);

        private readonly IContainer container;
        private readonly PageSize containerSize;
        private readonly Func<string, string> getImagePath;

        private readonly string html;

        private readonly TextStyle textStyle = TextStyle.Default
                .Fallback(y => y.FontFamily("MS Reference Sans Serif")
                .Fallback(y => y.FontFamily("Segoe UI Emoji")
                .Fallback(y => y.FontFamily("Microsoft YaHei"))));

        public HTMLQuestPDFBuilder(IContainer container, string html, Func<string, string> getImagePath, PageSize containerSize)
        {
            this.container = container;
            this.html = html;
            this.getImagePath = getImagePath;
            this.containerSize = containerSize;
        }

        public void Build()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(HTMLUtils.Clean(html));

            container.Column(col =>
            {
                Render(col, doc.DocumentNode, GetSpanForElement(""));
            });
        }

        private void RenderImg(ColumnDescriptor column, HtmlNode node, string? url = null)
        {
            var src = node.GetAttributeValue("src", "");
            var fullPath = getImagePath(src);

            var con = column.Item().AlignCenter();

            if (File.Exists(fullPath))
            {
                con.Element(e =>
                    {
                        using Image image = Image.Load(fullPath);
                        var requiredHeight = image.Height * (containerSize.Width / image.Width);
                        return requiredHeight > containerSize.Height ? e.MinHeight(containerSize.Height) : e;
                    })
                    .Element(e =>
                    {
                        return url == null ? e : e.Hyperlink(url);
                    })
                    .Image(fullPath, ImageScaling.FitArea);
            }
            else
            {
                con.Image(Placeholders.Image(200, 100), ImageScaling.FitArea);
            }
        }

        private void RenderText(ColumnDescriptor column, HtmlNode node, string? url = null, TextDescriptor? textDescriptor = null, TextSpan? spanAction = null)
        {
            var render = (TextDescriptor text) =>
            {
                var content = node.InnerText;

                if (content != " ")
                {
                    if (node.GetPrevNonEmptyNode()?.IsBlockNode() ?? true)
                    {
                        content = content.TrimStart();
                    }

                    if (node.GetNextNonEmptyNode()?.IsBlockNode() ?? true)
                    {
                        content = content.TrimEnd();
                    }
                }

                text.DefaultTextStyle(textStyle);
                var span = url == null ? text.Span(content) : text.Hyperlink(content, url);
                spanAction?.Invoke(span);
            };

            if (textDescriptor == null)
            {
                column.Item().Text(render);
            }
            else
            {
                render(textDescriptor);
            }
        }

        private void ApplyStyle(TextSpanDescriptor span, XElement inner)
        {
            // fill this dictionary with css expressions that map to QuestPDF methods
            var supportedStyles = new Dictionary<string, Dictionary<string, TextSpan>>()
            {
                ["font-weight"] = new Dictionary<string, TextSpan>()
                {
                    ["bold"] = (span) => span.Bold(),
                    ["normal"] = (span) => span.NormalWeight(),
                },
                ["font-style"] = new Dictionary<string, TextSpan>()
                {
                    ["italic"] = (span) => span.Italic(),
                    ["underline"] = (span) => span.Underline()
                }
            };

            var styles = inner.Attribute("style")?.Value.ToString();
            if (styles is null) return;

            var styleDictionary = styles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(item =>
            {
                var parts = item.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries); return new
                {
                    Key = parts[0].Trim(),
                    Value = parts[1].Trim()
                };
            }).ToDictionary(item => item.Key, item => item.Value);

            foreach (var style in styleDictionary)
            {
                // do we support this style attribute?
                if (supportedStyles.TryGetValue(style.Key, out var attribute))
                {
                    // do we support this style attribute value?
                    if (attribute.TryGetValue(style.Value, out var value))
                    {
                        value.Invoke(span);
                    }
                }
            }
        }

        private TextSpan GetSpanForElement(string elementName)
        {
            return elementName switch
            {
                "h1" => (span) => span.FontSize(32).Bold(),
                "h2" => (span) => span.FontSize(28).Bold(),
                "h3" => (span) => span.FontSize(24).Bold(),
                "h4" => (span) => span.FontSize(20).Bold(),
                "h5" => (span) => span.FontSize(16).Bold(),
                "h6" => (span) => span.FontSize(12).Bold(),
                "b" => (span) => span.Bold(),
                "i" => (span) => span.Italic(),
                "small" => (span) => span.Light(),
                "strike" => (span) => span.Strikethrough(),
                "s" => (span) => span.Strikethrough(),
                "u" => (span) => span.Underline(),
                _ => (span) => { }
            };
        }

        private Random random = new Random();

        private void RenderNodeChildren(ColumnDescriptor col, HtmlNode node, TextSpan spanAction, TextDescriptor? textDescriptor = null, string? url = null)
        {
            var renderChildren = (TextDescriptor? descriptor) =>
            {
                foreach (var child in node.ChildNodes)
                {
                    Render(col, child, (span) =>
                    {
                        spanAction?.Invoke(span);
                        GetSpanForElement(child.Name)?.Invoke(span);
                    }, descriptor, node.GetAttributeValue("href", null));
                }
            };

            var prevNode = node.GetPrevNonEmptyNode();

            if ((node.IsBlockNode() || prevNode != null && prevNode.IsBlockNode()) && !node.HasBlockOrImgElement() && !node.IsEmpty())
            {
                node.InnerHtml = node.InnerHtml.Trim();
                node.Id = String.IsNullOrEmpty(node.Id) ? Guid.NewGuid().ToString() : node.Id;

                col.Item().DebugArea(node.Name + " " + node.Id + " " + node.GetAttributeValue("class", ""), String.Format("#{0:X6}", random.Next(0x1000000)))
                    .ApplyBlockElementSettings(node.Name).Text(text => renderChildren(text));
            }
            else
            {
                renderChildren(textDescriptor);
            }
        }

        private void Render(ColumnDescriptor col, HtmlNode node, TextSpan spanAction, TextDescriptor? textDescriptor = null, string? url = null)
        {
            if (node.NodeType == HtmlNodeType.Comment) return;
            if (node.NodeType == HtmlNodeType.Text)
            {
                if (node.IsEmpty()) return;
                RenderText(col, node, url, textDescriptor, spanAction);
            }
            else
            {
                switch (node.Name)
                {
                    case "img":
                        RenderImg(col, node, url);
                        break;

                    default:
                        RenderNodeChildren(col, node, spanAction, textDescriptor, url);
                        break;
                }
            }
        }
    }
}