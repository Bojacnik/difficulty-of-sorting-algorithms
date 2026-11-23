using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DifficultyOfSortingAlgorithms.Core.Algorithms;

namespace DifficultyOfSortingAlgorithms.ViewModels;

/// <summary>An algorithm the user can tick for the next run.</summary>
public sealed partial class AlgorithmOption(ISortingAlgorithm algorithm, bool isSelected) : ObservableObject
{
    public ISortingAlgorithm Algorithm { get; } = algorithm;
    public string Name => Algorithm.Name;
    public string Complexity => Algorithm.Complexity;
    public string Description => Algorithm.Description;
    public string OriginText => Algorithm.Origin == AlgorithmOrigin.Bojalib ? "From Bojalib (dashed line)" : "Original implementation (solid line)";
    public IBrush Brush { get; } = new SolidColorBrush(AlgorithmStyle.ColorOf(algorithm));
    public AvaloniaList<double>? DashArray => AlgorithmStyle.IsDashed(Algorithm) ? AlgorithmStyle.Dashes : null;

    [ObservableProperty]
    private bool _isSelected = isSelected;
}
