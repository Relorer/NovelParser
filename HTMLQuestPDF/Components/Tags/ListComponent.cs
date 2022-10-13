using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class ListComponent : BaseHTMLComponent
    {
        public ListComponent(HtmlNode node) : base(node)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            var first = IsFirstList(node);
            return base.ApplyStyles(container).Element(e => first ? e.PaddingVertical(12) : e).PaddingLeft(30);
        }

        private bool IsFirstList(HtmlNode node)
        {
            if (node.ParentNode == null) return true;
            return !node.ParentNode.IsList() && IsFirstList(node.ParentNode);
        }
    }
}