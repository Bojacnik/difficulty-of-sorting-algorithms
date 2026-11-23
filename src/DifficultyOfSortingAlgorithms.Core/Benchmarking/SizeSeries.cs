namespace DifficultyOfSortingAlgorithms.Core.Benchmarking;

/// <summary>Array sizes to benchmark.</summary>
public static class SizeSeries
{
    /// <summary>Largest number of steps <see cref="Quadratic"/> accepts (100 · 1000² = 100 million elements).</summary>
    public const int MaxSteps = 1000;

    /// <summary>The original app's series: 100·i² for i = 1 … <paramref name="steps"/> (100, 400, 900, …).</summary>
    public static int[] Quadratic(int steps)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(steps, MaxSteps);
        var sizes = new int[steps];
        for (var i = 1; i <= steps; i++)
            sizes[i - 1] = 100 * i * i;
        return sizes;
    }
}
