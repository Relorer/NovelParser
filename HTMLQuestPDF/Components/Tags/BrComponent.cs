using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class BrComponent : BaseHTMLComponent
    {
        public BrComponent(HtmlNode node) : base(node)
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