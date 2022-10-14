using HtmlAgilityPack;
using HTMLQuestPDF.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class BrComponent : BaseHTMLComponent
    {
        public BrComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            return base.ApplyStyles(container);
        }

        protected override void ComposeSingle(IContainer container)
        {
            container.Text("");
        }
    }
}