using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using HTMLQuestPDF.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components
{
    internal delegate void ContainerAction(IContainer container);

    internal delegate void TextSpanAction(TextSpanDescriptor textSpan);

    internal class PDFComponent : IComponent
    {
        private readonly PageSize containerSize;

        private readonly Func<string, string> getImagePath;

        private readonly string html;

        public PDFComponent(string html, Func<string, string> getImagePath, PageSize containerSize)
        {
            this.html = html;
            this.getImagePath = getImagePath;
            this.containerSize = containerSize;
        }

        public void Compose(IContainer container)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(HTMLUtils.PrepareHTML(html));
            var node = doc.DocumentNode;

            CreateSeparateBranchesForTextNodes(node);

            container.Component(node.GetComponent());
        }

        /// <summary>
        /// Separate branches are created for block and text nodes located in the same linear node
        ///
        /// <p><s><div>div</div>text in s</s>text in p</p>
        /// to
        /// <p><s><div>div</div></s><s>text in s</s>text in p</p>
        ///
        /// This is necessary to avoid extra line breaks
        /// </summary>
        /// <param name="node"></param>
        private void CreateSeparateBranchesForTextNodes(HtmlNode node)
        {
            if (node.IsLineNode() && node.HasBlockElement())
            {
                var slices = node.GetSlices(new Models.Slice() { Nodes = new List<HtmlNode>() { node } });

                var nodes = new HtmlNodeCollection(node);

                var parent = node.ParentNode;
                var children = node.ParentNode.ChildNodes.ToList();

                foreach (var slice in slices)
                {
                    HtmlNode? newNode = null;

                    foreach (var item in slice.Nodes)
                    {
                        if (newNode == null)
                        {
                            newNode = item.CloneNode(false);
                            children.Insert(children.IndexOf(node), newNode);
                        }
                        else
                        {
                            var temp = item.CloneNode(false);
                            newNode.AppendChild(temp);
                            newNode = temp;
                        }
                    }

                    if (newNode != null)
                    {
                        newNode.InnerHtml = newNode.InnerText.Trim();
                    }
                }

                children.Remove(node);

                node.ParentNode.RemoveAllChildren();
                foreach (var item in children)
                {
                    parent.AppendChild(item);
                }
            }
            else
            {
                foreach (var item in node.ChildNodes.ToList())
                {
                    CreateSeparateBranchesForTextNodes(item);
                }
            }
        }
    }
}