namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>
/// Naive bubble sort: always n − 1 full passes over the whole array, even when it is already sorted.
/// It is the baseline the improved variant is compared against.
/// </summary>
public sealed class BubbleSort : ISortingAlgorithm
{
    /// <inheritdoc />
    public string Name => "Bubble sort";

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Original;

    /// <inheritdoc />
    public string Complexity => "O(n²)";

    /// <inheritdoc />
    public string Description => "Swaps neighbours that are out of order. Always makes n − 1 full passes, even over sorted data.";

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        long comparisons = 0, writes = 0;
        for (var pass = 0; pass < data.Length - 1; pass++)
        {
            for (var i = 0; i < data.Length - 1; i++)
            {
                comparisons++;
                if (data[i] > data[i + 1])
                {
                    (data[i], data[i + 1]) = (data[i + 1], data[i]);
                    writes += 2;
                }
            }
        }

        return new SortStats(comparisons, writes);
    }
}
