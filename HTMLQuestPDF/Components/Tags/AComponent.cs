using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class AComponent : BaseHTMLComponent
    {
        public AComponent(HtmlNode node) : base(node)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            container = base.ApplyStyles(container);
            return node.TryGetLink(out string link) ? container.Hyperlink(link) : container;
        }
    }
}