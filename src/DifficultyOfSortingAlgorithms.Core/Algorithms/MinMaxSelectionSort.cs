namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>
/// Double-ended selection sort: every pass finds both the minimum and the maximum of the unsorted middle and puts
/// them at its two ends, so it needs half as many passes as plain selection sort.
/// </summary>
/// <remarks>
/// The original found where the maximum went after the first swap by comparing values (<c>arr[min_i] == max</c>)
/// instead of positions. It happened to work, but only because of the scan order; this version tracks the index.
/// </remarks>
public sealed class MinMaxSelectionSort : ISortingAlgorithm
{
    /// <inheritdoc />
    public string Name => "Min-max selection sort";

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Original;

    /// <inheritdoc />
    public string Complexity => "O(n²)";

    /// <inheritdoc />
    public string Description => "Each pass finds the minimum and maximum of the unsorted middle and moves them to its ends.";

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        long comparisons = 0, writes = 0;
        for (int low = 0, high = data.Length - 1; low < high; low++, high--)
        {
            int minIndex = low, maxIndex = low;
            for (var k = low + 1; k <= high; k++)
            {
                comparisons += 2;
                if (data[k] < data[minIndex])
                    minIndex = k;
                if (data[k] > data[maxIndex])
                    maxIndex = k;
            }

            Swap(data, low, minIndex, ref writes);
            // If the maximum sat at `low`, the swap above just moved it to `minIndex`.
            if (maxIndex == low)
                maxIndex = minIndex;
            Swap(data, high, maxIndex, ref writes);
        }

        return new SortStats(comparisons, writes);
    }

    private static void Swap(Span<int> data, int a, int b, ref long writes)
    {
        if (a == b)
            return;
        (data[a], data[b]) = (data[b], data[a]);
        writes += 2;
    }
}
