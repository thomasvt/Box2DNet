public static class Extensions
{
    public static string ToCsv<T>(this IEnumerable<T> items, string separator = ", ") => string.Join(separator, items);
}