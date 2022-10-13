using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class PComponent : BaseHTMLComponent
    {
        public PComponent(HtmlNode node) : base(node)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            return base.ApplyStyles(container).PaddingVertical(6);
        }
    }
}