namespace DifficultyOfSortingAlgorithms.Core.Algorithms;

/// <summary>Where an algorithm's implementation comes from.</summary>
public enum AlgorithmOrigin
{
    /// <summary>Hand-written in this repository (the original school implementations).</summary>
    Original,

    /// <summary>Provided by the Bojalib package.</summary>
    Bojalib,
}

/// <summary>Operation counts collected while sorting. Only original algorithms count; Bojalib ones report <see cref="None"/>.</summary>
/// <param name="Comparisons">Number of element comparisons.</param>
/// <param name="Writes">Number of element writes into the array (a swap counts as two).</param>
public readonly record struct SortStats(long Comparisons, long Writes)
{
    /// <summary>No counts were collected.</summary>
    public static SortStats None => default;
}

/// <summary>A sorting algorithm that sorts integers in place in ascending order.</summary>
public interface ISortingAlgorithm
{
    /// <summary>Display name.</summary>
    string Name { get; }

    /// <summary>Who implemented it.</summary>
    AlgorithmOrigin Origin { get; }

    /// <summary>Expected complexity on random input, for display (for example "O(n²)").</summary>
    string Complexity { get; }

    /// <summary>One or two sentences on how it works, for tooltips.</summary>
    string Description { get; }

    /// <summary>Sorts <paramref name="data"/> in place.</summary>
    /// <returns>Operation counts, or <see cref="SortStats.None"/> if the algorithm does not count.</returns>
    SortStats Sort(Span<int> data);
}
