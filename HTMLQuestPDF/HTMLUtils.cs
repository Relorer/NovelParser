using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HTMLQuestPDF
{
    internal static class HTMLUtils
    {
        private static readonly string[] blockElements = new string[] { "#document", "div", "h1", "h2", "h3", "h4", "h5", "h6", "p" };

        // `text` auxiliary tag
        private static readonly string[] lineElements = new string[] { "i", "b", "small", "strike", "s", "u", "a", "text" };

        private static readonly string spaceAfterLineElementPattern = @$"\S<\/({string.Join("|", lineElements)})> ";

        public static string Clean(string value)
        {

            var result = RemoveExtraSpacesAndBreaks(value);
            return WrapSpacesAfterLineElement(result);
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

        public static bool HasBlockOrImgElement(this HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.IsBlockNode() || child.IsImg()) return true;
                if (HasBlockOrImgElement(child)) return true;
            }
            return false;
        }

        public static bool IsLink(this HtmlNode node)
        {
            return node.Name.ToLower() == "a";
        }

        public static bool IsImg(this HtmlNode node)
        {
            return node.Name.ToLower() == "img";
        }

        public static bool IsBlockNode(this HtmlNode node)
        {
            return blockElements.Contains(node.Name.ToLower());
        }

        public static bool IsEmpty(this HtmlNode node)
        {
            return string.IsNullOrEmpty(node.InnerHtml);
        }

        public static bool IsLineNode(this HtmlNode node)
        {
            return lineElements.Contains(node.Name.ToLower());
        }

        private static string RemoveExtraSpacesAndBreaks(string html)
        {
            var result = Regex.Replace(html, @"[ \r\n]+", " ");
            return RemoveSpacesBetweenElements(result);
        }

        private static string RemoveSpacesBetweenElements(string html)
        {
            return Regex.Replace(html, @$">\s+<", _ => @$"><");
        }

        private static string WrapSpacesAfterLineElement(string html)
        {
            return Regex.Replace(html, spaceAfterLineElementPattern, m => $"{m.Value.Substring(0, m.Value.Length - 1)}<text> </text>");
        }
    }
}