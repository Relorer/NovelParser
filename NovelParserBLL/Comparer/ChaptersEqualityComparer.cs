using NovelParserBLL.Models;

namespace NovelParserBLL.Comparer;

public class ChaptersEqualityComparer : IEqualityComparer<Chapter>
{
    public bool Equals(Chapter? x, Chapter? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Number == y.Number && x.Volume == y.Volume;
    }

    public int GetHashCode(Chapter obj)
    {
        return HashCode.Combine(obj.Number, obj.Volume);
    }
}
