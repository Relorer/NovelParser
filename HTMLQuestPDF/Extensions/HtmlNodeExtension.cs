using HtmlAgilityPack;
using HTMLQuestPDF.Components;
using HTMLQuestPDF.Components.Tags;
using HTMLQuestPDF.Models;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Extensions
{
    internal static class HtmlNodeExtension
    {
        public static List<Block> GetBlocks(this HtmlNode node)
        {
            var slices = node.GetSlices(new Slice() { Nodes = new List<HtmlNode>() { node } });

            Block buffer = new Block();
            var blocks = new List<Block>();

            foreach (var item in slices)
            {
                if (!buffer!.Slices.Any() || buffer!.Slices.Last().BaseBlock == item.BaseBlock)
                {
                    buffer.Slices.Add(item);
                }
                else
                {
                    blocks.Add(buffer);
                    buffer = new Block() { Slices = new List<Slice>() { item } };
                }
            }
            blocks.Add(buffer);

            blocks.ForEach(b => b.Trim());

            return blocks.Where(s => s.HasContent()).ToList();
        }

        public static IComponent GetComponent(this HtmlNode node, HTMLComponentsArgs args)
        {
            return node.Name.ToLower() switch
            {
                "#text" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "b" or "s" or "strike" or "i" or "small" or "u"
                    => new ParagraphComponent(new List<HtmlNode>() { node }),
                "br" => new BrComponent(node, args),
                "a" => new AComponent(node, args),
                "div" => new BaseHTMLComponent(node, args),
                "p" => new PComponent(node, args),
                "table" => new TableComponent(node, args),
                "ul" or "ol" => new ListComponent(node, args),
                "img" => new ImgComponent(node, args),
                _ => new BaseHTMLComponent(node, args),
            };
        }

        public static HtmlNode? GetListNode(this HtmlNode node)
        {
            if (node.IsList()) return node;
            if (node.ParentNode == null) return null;
            return GetListNode(node.ParentNode);
        }

        public static HtmlNode? GetNextNonEmptyNode(this HtmlNode node)
        {
            var currentNode = node.NextSibling;
            while (currentNode?.NodeType == HtmlNodeType.Text && (currentNode?.InnerText == null || currentNode?.InnerText == " "))
            {
                if (currentNode == null) return null;
                currentNode = currentNode.NextSibling;
            }
            return currentNode;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <returns>
        /// -1 - not a list
        /// 0 - marked list
        /// > 0 - number in the list
        /// </returns>
        public static int GetNumberInList(this HtmlNode node)
        {
            HtmlNode? listItem = null;

            if (node != null && node.IsListItem()) listItem = node;
            if (node?.ParentNode != null && node.ParentNode.IsListItem()) listItem = node.ParentNode;

            if (listItem != null)
            {
                var listNode = listItem.GetListNode();
                if (listNode == null || listNode.IsMarkedList()) return 0;

                return listNode.Descendants("li").Where(n => n.GetListNode() == listNode).ToList().IndexOf(listItem) + 1;
            }
            return -1;
        }

        public static HtmlNode? GetPrevNonEmptyNode(this HtmlNode node)
        {
            var currentNode = node.PreviousSibling;
            while (currentNode?.NodeType == HtmlNodeType.Text && (currentNode?.InnerText == null || currentNode?.InnerText == " "))
            {
                if (currentNode == null) return null;
                currentNode = currentNode.PreviousSibling;
            }
            return currentNode;
        }

        public static List<Slice> GetSlices(this HtmlNode node, Slice slice)
        {
            var result = new List<Slice>();

            if (!node.ChildNodes.Any() || node.NodeType == HtmlNodeType.Text)
            {
                result.Add(slice);
                return result;
            }
            else
            {
                foreach (var item in node.ChildNodes)
                {
                    result.AddRange(GetSlices(item, slice with { Nodes = new List<HtmlNode>(slice.Nodes) { item } }));
                }
            }

            return result;
        }

        public static bool HasBlockElement(this HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                return child.IsBlockNode() || HasBlockElement(child);
            }
            return false;
        }

        public static bool HasContent(this HtmlNode node)
        {
            foreach (var item in node.ChildNodes)
            {
                if (HasContent(item)) return true;
            }
            return !node.IsEmpty();
        }

        public static bool IsBlockNode(this HtmlNode node)
        {
            return StyleSettings.BlockElements.Contains(node.Name.ToLower());
        }

        public static bool IsBr(this HtmlNode node)
        {
            return node.Name.ToLower() == "br";
        }

        public static bool IsTr(this HtmlNode node)
        {
            return node.Name.ToLower() == "tr";
        }

        public static bool IsEmpty(this HtmlNode node)
        {
            return string.IsNullOrEmpty(node.InnerText) && !node.IsImg() && !node.IsBr();
        }

        public static bool IsImg(this HtmlNode node)
        {
            return node.Name.ToLower() == "img";
        }

        public static bool IsTable(this HtmlNode node)
        {
            return node.Name.ToLower() == "table";
        }

        public static bool IsLineNode(this HtmlNode node)
        {
            return StyleSettings.LineElements.Contains(node.Name.ToLower());
        }

        public static bool IsLink(this HtmlNode node)
        {
            return node.Name.ToLower() == "a";
        }

        public static bool IsList(this HtmlNode node)
        {
            return node.IsMarkedList() || node.IsNumberedList();
        }

        public static bool IsListItem(this HtmlNode node)
        {
            return node.Name.ToLower() == "li";
        }

        public static bool IsMarkedList(this HtmlNode node)
        {
            return node.Name.ToLower() == "ul";
        }

        public static bool IsNumberedList(this HtmlNode node)
        {
            return node.Name.ToLower() == "ol";
        }

        public static bool TryGetLink(this HtmlNode node, out string url)
        {
            var current = node;
            while (node != null)
            {
                if (node.IsLink())
                {
                    url = node.GetAttributeValue("href", "");
                    return true;
                }
                current = node.ParentNode;
            }

            url = "";
            return false;
        }
    }
}