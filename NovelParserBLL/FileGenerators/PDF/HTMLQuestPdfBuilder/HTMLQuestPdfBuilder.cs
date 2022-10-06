using AngleSharp.Dom;
using HtmlAgilityPack;
using NovelParserBLL.Extensions;
using NovelParserBLL.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Xml.Linq;

namespace NovelParserBLL.FileGenerators.PDF.HTMLQuestPdfBuilder
{
    internal abstract class HTMLQuestPdfBuilder
    {
        protected readonly Chapter chapter;
        private readonly IDocumentContainer container;

        protected HTMLQuestPdfBuilder(IDocumentContainer container, Chapter chapter)
        {
            this.chapter = chapter;
            this.container = container;
        }

        public void Build()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(chapter.Content);

            container.Page(page =>
            {
                GetContentContainer(page).Column(col =>
                {
                    Render(container, col, doc.DocumentNode, GetSpanForElement(""));
                });
            });
        }

        protected abstract IContainer GetContentContainer(PageDescriptor page);

        private void Render(IDocumentContainer container, ColumnDescriptor col, HtmlNode node, Action<TextSpanDescriptor> spanAction, TextDescriptor? textDescriptor = null)
        {
            if (node.NodeType == HtmlNodeType.Text && !string.IsNullOrEmpty(node.InnerHtml.Clean()))
            {
                var renderText = (TextDescriptor text) =>
                {
                    var span = text.Span(node.InnerText.Clean());
                    spanAction?.Invoke(span);
                };

                if (textDescriptor == null)
                {
                    col.Item().Text(renderText);
                }
                else
                {
                    renderText(textDescriptor);
                }
            }
            else
            {
                var parseChildren = (TextDescriptor? descriptor) =>
                {
                    foreach (var child in node.ChildNodes)
                    {
                        Render(container, col, child, (span) =>
                        {
                            spanAction?.Invoke(span);
                            GetSpanForElement(child.Name)?.Invoke(span);
                        }, descriptor);
                    }
                };

                switch (node.Name)
                {
                    case "img":
                        RenderImg(col, node);
                        break;

                    default:
                        var element = GetContainerElement(node.Name, col);
                        if (element == null) parseChildren(textDescriptor);
                        else element.Text(text => parseChildren(text));
                        break;
                }
            }
        }

        protected abstract void RenderImg(ColumnDescriptor column, HtmlNode node);

        protected abstract IContainer? GetContainerElement(string elementName, ColumnDescriptor column);

        private Action<TextSpanDescriptor> GetSpanForElement(string elementName)
        {
            return elementName switch
            {
                "h1" => (span) => span.AddFallbackFontFamily().FontSize(32),
                "h2" => (span) => span.AddFallbackFontFamily().FontSize(28),
                "h3" => (span) => span.AddFallbackFontFamily().FontSize(24),
                "h4" => (span) => span.AddFallbackFontFamily().FontSize(20),
                "h5" => (span) => span.AddFallbackFontFamily().FontSize(16),
                "h6" => (span) => span.AddFallbackFontFamily().FontSize(12),
                "b" => (span) => span.AddFallbackFontFamily().Bold(),
                "i" => (span) => span.AddFallbackFontFamily().Italic(),
                "small" => (span) => span.AddFallbackFontFamily().Light(),
                "strike" => (span) => span.AddFallbackFontFamily().Strikethrough(),
                "s" => (span) => span.AddFallbackFontFamily().Strikethrough(),
                "u" => (span) => span.AddFallbackFontFamily().Underline(),
                _ => (span) => span.AddFallbackFontFamily()
            };
        }

        private void ApplyStyle(TextSpanDescriptor span, XElement inner)
        {
            // fill this dictionary with css expressions that map to QuestPDF methods
            var supportedStyles = new Dictionary<string, Dictionary<string, Action<TextSpanDescriptor>>>()
            {
                ["font-weight"] = new Dictionary<string, Action<TextSpanDescriptor>>()
                {
                    ["bold"] = (span) => span.Bold(),
                    ["normal"] = (span) => span.NormalWeight(),
                },
                ["font-style"] = new Dictionary<string, Action<TextSpanDescriptor>>()
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
    }
}