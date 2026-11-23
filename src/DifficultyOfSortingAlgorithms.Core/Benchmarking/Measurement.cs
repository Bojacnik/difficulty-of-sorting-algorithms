using DifficultyOfSortingAlgorithms.Core.Algorithms;

namespace DifficultyOfSortingAlgorithms.Core.Benchmarking;

/// <summary>How long one algorithm took to sort one array size.</summary>
/// <param name="Algorithm">The algorithm measured.</param>
/// <param name="Size">Number of elements sorted.</param>
/// <param name="Milliseconds">Median wall-clock time over all repetitions.</param>
/// <param name="Stats">Operation counts from the median run, or <see cref="SortStats.None"/>.</param>
public sealed record Measurement(ISortingAlgorithm Algorithm, int Size, double Milliseconds, SortStats Stats);

/// <summary>Benchmark settings.</summary>
/// <param name="Sizes">Array sizes, ascending.</param>
/// <param name="Input">Shape of the input arrays.</param>
/// <param name="Repetitions">Runs per size; the median time is reported.</param>
/// <param name="TimeBudget">
/// Once one run of an algorithm takes longer than this, it is not run on larger sizes (an O(n²) sort on millions of
/// elements would otherwise run for hours).
/// </param>
/// <param name="Seed">Seed for the input arrays, so runs can be repeated.</param>
public sealed record BenchmarkOptions(
    IReadOnlyList<int> Sizes,
    InputKind Input = InputKind.Random,
    int Repetitions = 3,
    TimeSpan? TimeBudget = null,
    int Seed = 42)
{
    /// <summary>The budget used when <see cref="TimeBudget"/> is not set.</summary>
    public static TimeSpan DefaultTimeBudget { get; } = TimeSpan.FromSeconds(2);
}
