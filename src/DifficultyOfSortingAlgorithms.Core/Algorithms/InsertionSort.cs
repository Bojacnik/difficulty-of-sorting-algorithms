namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>Classic insertion sort: shifts larger elements right and drops each element into its gap.</summary>
public sealed class InsertionSort : ISortingAlgorithm
{
    /// <inheritdoc />
    public string Name => "Insertion sort";

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Original;

    /// <inheritdoc />
    public string Complexity => "O(n²), O(n) sorted";

    /// <inheritdoc />
    public string Description => "Takes each element and shifts it left into place. Very fast on nearly sorted data.";

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        long comparisons = 0, writes = 0;
        for (var i = 1; i < data.Length; i++)
        {
            var value = data[i];
            var j = i - 1;
            while (j >= 0)
            {
                comparisons++;
                if (data[j] <= value)
                    break;
                data[j + 1] = data[j];
                writes++;
                j--;
            }

            data[j + 1] = value;
            writes++;
        }

        return new SortStats(comparisons, writes);
    }
}
