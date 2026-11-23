namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>Top-down, stable merge sort.</summary>
/// <remarks>
/// The original allocated two new arrays on every merge (about 2n allocations per sort), which made the timings
/// mostly measure the garbage collector. This version allocates one scratch buffer per sort.
/// </remarks>
public sealed class MergeSort : ISortingAlgorithm
{
    /// <inheritdoc />
    public string Name => "Merge sort";

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Original;

    /// <inheritdoc />
    public string Complexity => "O(n log n)";

    /// <inheritdoc />
    public string Description => "Splits the array in halves, sorts each and merges them. Stable, needs n extra memory.";

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        if (data.Length < 2)
            return default;

        var stats = new Counter();
        var scratch = new int[data.Length];
        SortRange(data, scratch, ref stats);
        return new SortStats(stats.Comparisons, stats.Writes);
    }

    private struct Counter
    {
        public long Comparisons;
        public long Writes;
    }

    private static void SortRange(Span<int> data, Span<int> scratch, ref Counter stats)
    {
        if (data.Length < 2)
            return;

        var middle = data.Length / 2;
        SortRange(data[..middle], scratch[..middle], ref stats);
        SortRange(data[middle..], scratch[middle..], ref stats);
        Merge(data, middle, scratch, ref stats);
    }

    private static void Merge(Span<int> data, int middle, Span<int> scratch, ref Counter stats)
    {
        // Copy the left half out; the right half is read in place, the merge writes back over `data`.
        var left = scratch[..middle];
        data[..middle].CopyTo(left);

        int i = 0, j = middle, k = 0;
        while (i < left.Length && j < data.Length)
        {
            stats.Comparisons++;
            // `<=` keeps equal elements in their original order (stability).
            data[k++] = left[i] <= data[j] ? left[i++] : data[j++];
            stats.Writes++;
        }

        // Whatever remains of the right half is already in place.
        while (i < left.Length)
        {
            data[k++] = left[i++];
            stats.Writes++;
        }
    }
}
