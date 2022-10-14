using HtmlAgilityPack;
using HTMLQuestPDF.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class PComponent : BaseHTMLComponent
    {
        public PComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            return base.ApplyStyles(container).PaddingVertical(6);
        }
    }
}