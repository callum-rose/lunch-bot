namespace LunchBot;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, int? seed = null)
    {
        Random random = seed.HasValue ? new Random(seed.Value) : new Random();
        return enumerable.OrderBy(_ => random.Next());
    }
}