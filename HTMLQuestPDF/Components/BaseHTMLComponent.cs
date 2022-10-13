using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components
{
    internal class BaseHTMLComponent : IComponent
    {
        protected readonly HtmlNode node;

        public BaseHTMLComponent(HtmlNode node)
        {
            this.node = node;
        }

        public void Compose(IContainer container)
        {
            if (!node.HasContent()) return;

            container = ApplyStyles(container);

            if (node.ChildNodes.Any())
            {
                ComposeMany(container);
            }
            else
            {
                ComposeSingle(container);
            }
        }

        protected virtual IContainer ApplyStyles(IContainer container)
        {
            return container.DefaultTextStyle(StyleSettings.TextStyle);
        }

        protected virtual void ComposeSingle(IContainer container)
        {
        }

        protected virtual void ComposeMany(IContainer container)
        {
            container.Column(col =>
            {
                var buffer = new List<HtmlNode>();
                foreach (var item in node.ChildNodes)
                {
                    if (item.IsBlockNode() || item.HasBlockElement())
                    {
                        ComposeMany(col, buffer);
                        buffer.Clear();

                        col.Item().Component(item.GetComponent());
                    }
                    else
                    {
                        buffer.Add(item);
                    }
                }
                ComposeMany(col, buffer);
            });
        }

        private void ComposeMany(ColumnDescriptor col, List<HtmlNode> nodes)
        {
            if (nodes.Count == 1)
            {
                col.Item().Component(nodes.First().GetComponent());
            }
            else if (nodes.Count > 0)
            {
                col.Item().Component(new ParagraphComponent(nodes));
            }
        }
    }
}