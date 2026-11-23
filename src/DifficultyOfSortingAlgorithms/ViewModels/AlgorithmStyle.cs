using Avalonia.Collections;
using Avalonia.Media;
using DifficultyOfSortingAlgorithms.Core.Algorithms;

namespace DifficultyOfSortingAlgorithms.ViewModels;

/// <summary>
/// The colour and line style for each algorithm. An original and its Bojalib counterpart (bubble, insertion,
/// merge) share a colour; the original is solid and the Bojalib one dashed. Every other algorithm has its own hue.
/// </summary>
public static class AlgorithmStyle
{
    private static readonly Dictionary<Type, string> OriginalColors = new()
    {
        [typeof(BubbleSort)] = "#3D8BFF",          // blue
        [typeof(ImprovedBubbleSort)] = "#FF7A1A",  // orange
        [typeof(InsertionSort)] = "#2ED573",       // green
        [typeof(MinMaxSelectionSort)] = "#FFD60A", // yellow
        [typeof(MergeSort)] = "#B57BFF",           // violet
    };

    private static readonly Dictionary<BojalibAlgorithm, string> BojalibColors = new()
    {
        [BojalibAlgorithm.Bubble] = "#3D8BFF",     // same as bubble sort
        [BojalibAlgorithm.Insertion] = "#2ED573",  // same as insertion sort
        [BojalibAlgorithm.Merge] = "#B57BFF",      // same as merge sort
        [BojalibAlgorithm.Heap] = "#FF3B6B",       // red-pink
        [BojalibAlgorithm.Quick] = "#F2F4F8",      // white
        [BojalibAlgorithm.Radix] = "#00E0FF",      // cyan
        [BojalibAlgorithm.Parallel] = "#C69C6D",   // tan
    };

    /// <summary>Dash pattern for Bojalib lines, in multiples of the stroke width.</summary>
    public static AvaloniaList<double> Dashes { get; } = [3, 2];

    public static Color ColorOf(ISortingAlgorithm algorithm) => Color.Parse(algorithm switch
    {
        BojalibAlgorithm b when BojalibColors.TryGetValue(b, out var c) => c,
        _ when OriginalColors.TryGetValue(algorithm.GetType(), out var c) => c,
        _ => "#8B95A5",
    });

    public static bool IsDashed(ISortingAlgorithm algorithm) => algorithm.Origin == AlgorithmOrigin.Bojalib;
}
