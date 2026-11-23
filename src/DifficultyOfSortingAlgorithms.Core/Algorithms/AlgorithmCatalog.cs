namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>Every algorithm the app can benchmark.</summary>
public static class AlgorithmCatalog
{
    /// <summary>The original hand-written algorithms.</summary>
    public static IReadOnlyList<ISortingAlgorithm> Original { get; } =
    [
        new BubbleSort(),
        new ImprovedBubbleSort(),
        new InsertionSort(),
        new MinMaxSelectionSort(),
        new MergeSort(),
    ];

    /// <summary>Algorithms from the Bojalib package.</summary>
    public static IReadOnlyList<ISortingAlgorithm> Bojalib { get; } =
    [
        BojalibAlgorithm.Bubble,
        BojalibAlgorithm.Insertion,
        BojalibAlgorithm.Heap,
        BojalibAlgorithm.Merge,
        BojalibAlgorithm.Quick,
        BojalibAlgorithm.Radix,
        BojalibAlgorithm.Parallel,
    ];

    /// <summary>All algorithms, originals first.</summary>
    public static IReadOnlyList<ISortingAlgorithm> All { get; } = [.. Original, .. Bojalib];
}
