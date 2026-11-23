using B = global::Bojalib.Sorting;

namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>Bojalib's sorting algorithms behind <see cref="ISortingAlgorithm"/>. They do not count operations.</summary>
public sealed class BojalibAlgorithm : ISortingAlgorithm
{
    private readonly SpanAction _sort;

    private delegate void SpanAction(Span<int> data);

    private BojalibAlgorithm(string name, string complexity, string description, SpanAction sort)
    {
        Name = name;
        Complexity = complexity;
        Description = description;
        _sort = sort;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public AlgorithmOrigin Origin => AlgorithmOrigin.Bojalib;

    /// <inheritdoc />
    public string Complexity { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public SortStats Sort(Span<int> data)
    {
        _sort(data);
        return SortStats.None;
    }

    /// <summary>Bojalib bubble sort.</summary>
    public static BojalibAlgorithm Bubble { get; } = new("Bojalib bubble sort", "O(n²)", "Bojalib's bubble sort, with the same early exit as the improved version.", B.BubbleSort.Sort);

    /// <summary>Bojalib insertion sort.</summary>
    public static BojalibAlgorithm Insertion { get; } = new("Bojalib insertion sort", "O(n²)", "Bojalib's stable, in-place insertion sort. Fastest choice for small or nearly sorted data.", B.InsertionSort.Sort);

    /// <summary>Bojalib heapsort.</summary>
    public static BojalibAlgorithm Heap { get; } = new("Bojalib heapsort", "O(n log n)", "Builds a binary heap, then repeatedly moves its maximum to the end. In place, never worse than n log n.", B.HeapSort.Sort);

    /// <summary>Bojalib stable, adaptive merge sort (TimSort-style).</summary>
    public static BojalibAlgorithm Merge { get; } = new("Bojalib merge sort", "O(n log n)", "Stable, adaptive merge sort (TimSort-style): finds existing sorted runs and merges them.", B.MergeSort.Sort);

    /// <summary>Bojalib pdqsort with SIMD partitioning.</summary>
    public static BojalibAlgorithm Quick { get; } = new("Bojalib quicksort (pdq)", "O(n log n)", "Pattern-defeating quicksort with SIMD partitioning. The fastest single-threaded sort here.", B.QuickSort.Sort);

    /// <summary>Bojalib LSD radix sort.</summary>
    public static BojalibAlgorithm Radix { get; } = new("Bojalib radix sort", "O(n)", "Sorts integers digit by digit (byte by byte) without comparing them.", B.RadixSort.Sort);

    /// <summary>Bojalib multithreaded sample sort.</summary>
    public static BojalibAlgorithm Parallel { get; } = new(
        "Bojalib parallel sort",
        "O(n log n / p)",
        "Multithreaded sample sort: splits values into buckets and sorts them on all CPU cores. Slower on small arrays.",
        data =>
        {
            // ParallelSort needs Memory<T>, which a Span cannot become; sort a copy and write it back.
            var copy = data.ToArray();
            B.ParallelSort.Sort(copy);
            copy.CopyTo(data);
        });
}
