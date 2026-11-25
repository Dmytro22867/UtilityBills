namespace UtilityBills.Common;

public class CatalogFilter<T>
{
    public List<T> FindAll(IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate).ToList();

    public int CountMatching(IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Count(predicate);
}
