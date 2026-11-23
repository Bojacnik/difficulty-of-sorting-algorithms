using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using DifficultyOfSortingAlgorithms.Core.Algorithms;
using DifficultyOfSortingAlgorithms.Core.Benchmarking;

namespace DifficultyOfSortingAlgorithms.ViewModels;

/// <summary>One line in the chart: an algorithm's time (y, ms) against array size (x).</summary>
public sealed class ChartSeries(ISortingAlgorithm algorithm)
{
    public ISortingAlgorithm Algorithm { get; } = algorithm;
    public string Name => Algorithm.Name;
    public Color Color { get; } = AlgorithmStyle.ColorOf(algorithm);
    public IBrush Brush { get; } = new SolidColorBrush(AlgorithmStyle.ColorOf(algorithm));
    public bool IsDashed => AlgorithmStyle.IsDashed(Algorithm);
    public AvaloniaList<double>? DashArray => IsDashed ? AlgorithmStyle.Dashes : null;
    public List<Measurement> Measurements { get; } = [];
    public IEnumerable<Point> Points => Measurements.Select(m => new Point(m.Size, m.Milliseconds));
}
