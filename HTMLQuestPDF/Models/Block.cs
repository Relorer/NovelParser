using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;

namespace HTMLQuestPDF.Models
{
    record Block
    {
        public List<Slice> Slices { get; init; } = new List<Slice>();

        public bool HasContent()
        {
            return Slices.Select(n => n.HasContent()).Aggregate((a, b) => a || b);
        }
        public HtmlNode? BaseBlock => Slices.Last().BaseBlock;

        public void Trim()
        {
            var startEdge = Slices.First().Node;
            var endEdge = Slices.Last().Node;
            startEdge.InnerHtml = startEdge.InnerHtml.TrimStart();
            endEdge.InnerHtml = endEdge.InnerHtml.TrimEnd();
            BrToText();
        }

        private void BrToText()
        {
            Slices.Where(s => s.Node.IsBr()).ToList().ForEach(s => s.Node.InnerHtml = "\n");
            if (Slices.Where(s => s.Node.IsBr()).Count() == Slices.Count)
            {
                Slices.First().Node.InnerHtml = " ";
            }
        }

        public string InnerHtml()
        {
            return Slices.Select(s => s.Node.InnerHtml).Aggregate((a, b) => a + b);
        }
    }
}