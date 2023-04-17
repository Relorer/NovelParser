using System.Globalization;
using NovelParserBLL.Models;

namespace NovelParserBLL.Comparer;

public class ChaptersComparer : IComparer<Chapter>
{
    public int Compare(Chapter? x, Chapter? y)
    {
        if (x == y) return 0;
        if (y is null) return 1;
        if (x?.Number is null) return -1;
        if (y.Number is null) return 1;

        if (x.Volume > y.Volume) return 1;
        if (x.Volume < y.Volume) return -1;

        var xVal = ToFloat(x.Number);
        var yVal = ToFloat(y.Number);
        return xVal.CompareTo(yVal);
    }

    private static float ToFloat(string value)
    {
        value = value.Replace(",",".");
        return float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result)
            ? result : 0f;
    }
}
