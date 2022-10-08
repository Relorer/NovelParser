using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF
{
    public static class IDocumentContainerHTMLExtension
    {
        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, PageSize pageSize, float marginLeft, float marginTop, float marginRight, float marginBottom, Unit unit = Unit.Point)
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.MarginLeft(marginLeft, unit);
                page.MarginTop(marginTop, unit);
                page.MarginRight(marginRight, unit);
                page.MarginBottom(marginBottom, unit);

                var pageSizeWithoutMargins = new PageSize(pageSize.Width - ToPoints(marginLeft, unit) - ToPoints(marginRight, unit), pageSize.Height - ToPoints(marginTop, unit) - ToPoints(marginBottom, unit));
                new HTMLQuestPDFBuilder(page.Content(), html, getImagePath, pageSizeWithoutMargins).Build();
            });
        }

        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, float width, float marginLeft, float marginTop, float marginRight, float marginBottom, Unit unit = Unit.Point)
        {
            container.Page(page =>
            {
                page.ContinuousSize(width);
                page.MarginLeft(marginLeft, unit);
                page.MarginTop(marginTop, unit);
                page.MarginRight(marginRight, unit);
                page.MarginBottom(marginBottom, unit);

                var pageSizeWithoutMargins = new PageSize(ToPoints(width, unit) - ToPoints(marginLeft, unit) - ToPoints(marginRight, unit), 14400 - ToPoints(marginTop, unit) - ToPoints(marginBottom, unit));
                new HTMLQuestPDFBuilder(page.Content(), html, getImagePath, pageSizeWithoutMargins).Build();
            });
        }

        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, PageSize pageSize, float marginHorizontal, float marginVertical, Unit unit = Unit.Point)
        {
            container.HTMLPage(html, getImagePath, pageSize, marginHorizontal, marginVertical, marginHorizontal, marginVertical, unit);
        }

        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, PageSize pageSize, float margin, Unit unit = Unit.Point)
        {
            container.HTMLPage(html, getImagePath, pageSize, margin, margin, margin, margin, unit);
        }

        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, float width, float marginHorizontal, float marginVertical, Unit unit = Unit.Point)
        {
            container.HTMLPage(html, getImagePath, width, marginHorizontal, marginVertical, marginHorizontal, marginVertical, unit);
        }

        public static void HTMLPage(this IDocumentContainer container, string html, Func<string, string> getImagePath, float width, float margin, Unit unit = Unit.Point)
        {
            container.HTMLPage(html, getImagePath, width, margin, margin, margin, margin, unit);
        }

        private static float ToPoints(float value, Unit unit)
        {
            return value * GetConversionFactor();
            float GetConversionFactor()
            {
                return unit switch
                {
                    Unit.Point => 1f,
                    Unit.Meter => 2834.64575f,
                    Unit.Centimetre => 28.3464565f,
                    Unit.Millimetre => 2.83464575f,
                    Unit.Feet => 864f,
                    Unit.Inch => 72f,
                    Unit.Mill => 0.072f,
                    _ => throw new ArgumentOutOfRangeException("unit", unit, null),
                };
            }
        }
    }
}