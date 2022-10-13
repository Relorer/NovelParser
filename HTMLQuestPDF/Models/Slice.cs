using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;

namespace HTMLQuestPDF.Models
{
    internal record Slice
    {
        public List<HtmlNode> Nodes { get; init; } = new List<HtmlNode>();

        public HtmlNode Node => Nodes.Last();

        public HtmlNode? BaseBlock => Nodes.FindLast(n => n.IsBlockNode());

        public bool HasContent()
        {
            return Node.HasContent();
        }

        public bool IsListElement => Nodes.Find(n => n.IsList()) != null;

        public bool TryGetLink(out string href)
        {
            href = "";

            foreach (var item in Nodes.Reverse<HtmlNode>())
            {
                if (item.IsLink())
                {
                    href = item.GetAttributeValue("href", null);
                }
            }

            return !string.IsNullOrEmpty(href);
        }
    }
}