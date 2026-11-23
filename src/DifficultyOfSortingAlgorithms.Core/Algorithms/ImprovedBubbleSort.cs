namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>
/// Bubble sort with both standard improvements: each pass stops before the already-sorted tail, and the sort ends
/// as soon as a pass makes no swap. Sorted input therefore takes a single O(n) pass.
/// </summary>
/// <remarks>
/// The original version only shortened the passes; it had no early exit, so it was still Θ(n²) on sorted input.
/// </remarks>
public sealed class ImprovedBubbleSort : ISortingAlgorithm
{
    /// <inheritdoc />
    public string Name => "Improved bubble sort";

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Original;

    /// <inheritdoc />
    public string Complexity => "O(n²), O(n) sorted";

    /// <inheritdoc />
    public string Description => "Bubble sort that skips the sorted tail and stops after a pass with no swap. One pass on sorted data.";

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        long comparisons = 0, writes = 0;
        // Everything at or after `end` is in its final place. After a pass, the last swap position is the new end.
        var end = data.Length - 1;
        while (end > 0)
        {
            var lastSwap = 0;
            for (var i = 0; i < end; i++)
            {
                comparisons++;
                if (data[i] > data[i + 1])
                {
                    (data[i], data[i + 1]) = (data[i + 1], data[i]);
                    writes += 2;
                    lastSwap = i;
                }
            }

            end = lastSwap;
        }

        return new SortStats(comparisons, writes);
    }
}
