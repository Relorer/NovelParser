using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class TableComponent : BaseHTMLComponent
    {
        public TableComponent(HtmlNode node) : base(node)
        {
        }

        protected override void ComposeMany(IContainer container)
        {
            container.Debug("")
                .Border(1)
                .Background(Colors.Grey.Lighten3)
                .MinWidth(50)
                .MinHeight(50)
                .AlignCenter()
                .AlignMiddle()
                .Table(table =>
                {
                    var tableItems = node.SelectNodes("(//th | //td)");

                    List<List<HtmlNode>> lines = new List<List<HtmlNode>>();

                    List<HtmlNode> lastLine = new List<HtmlNode>();
                    HtmlNode? lastTr = GetTr(tableItems.First());

                    foreach (var item in tableItems)
                    {
                        var currentTr = GetTr(item);
                        if (lastTr != currentTr)
                        {
                            lines.Add(lastLine);
                            lastLine = new List<HtmlNode>();
                            lastTr = currentTr;
                        }
                        lastLine.Add(item);
                    }

                    if (lastLine != null) lines.Add(lastLine);

                    var maxColumns = lines.Max(l => l.Select(n => n.GetAttributeValue("colspan", 1)).Aggregate((a, b) => a + b));

                    table.ColumnsDefinition(columns =>
                    {
                        for (int i = 0; i < maxColumns; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    var rows = new List<bool[]>()
                    {
                        new bool[maxColumns]
                    };

                    var getNextPosition = (uint colSpan, uint rowSpan) =>
                    {
                        uint col = 0;
                        uint row = 0;

                        return (col, row);
                    };

                    foreach (var line in lines)
                    {
                        foreach (var cell in line)
                        {
                            uint colSpan = (uint)cell.GetAttributeValue("colspan", 1);
                            uint rowSpan = (uint)cell.GetAttributeValue("rowspan", 1);

                            (uint col, uint row) = getNextPosition(colSpan, rowSpan);

                            table.Cell()
                            .ColumnSpan(colSpan)
                            .Column(col)
                            .Row(row)
                            .RowSpan(rowSpan)
                            .Component(cell.GetComponent());
                        }
                    }
                });
        }

        private HtmlNode? GetTr(HtmlNode node)
        {
            if (node.IsTable() || node == null) return null;
            return node.IsTr() ? node : GetTr(node.ParentNode);
        }
    }
}