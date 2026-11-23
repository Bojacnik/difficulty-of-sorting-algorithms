using System.Diagnostics;
using Bojalib.Sorting;
using DifficultyOfSortingAlgorithms.Core.Algorithms;

namespace DifficultyOfSortingAlgorithms.Core.Benchmarking;

/// <summary>Runs algorithms on growing arrays and measures how long each takes.</summary>
/// <remarks>
/// Fixes over the original app: every algorithm sorts the very same input for a given size (it used to get its own
/// random array), times are sub-millisecond doubles instead of truncated <c>ElapsedMilliseconds</c>, each result is
/// checked to really be sorted, the JIT is warmed up first, and slow algorithms drop out instead of freezing the app.
/// </remarks>
public static class SortBenchmark
{
    /// <summary>Thrown result check: the algorithm returned an unsorted array.</summary>
    public sealed class UnsortedResultException(ISortingAlgorithm algorithm, int size)
        : Exception($"{algorithm.Name} did not sort an array of {size} elements.")
    {
        /// <summary>The broken algorithm.</summary>
        public ISortingAlgorithm Algorithm { get; } = algorithm;
    }

    /// <summary>
    /// Benchmarks <paramref name="algorithms"/>. Sizes run in ascending order; within a size, all algorithms sort
    /// copies of the same arrays. Each finished measurement is reported to <paramref name="progress"/> right away.
    /// </summary>
    /// <exception cref="UnsortedResultException">An algorithm produced an unsorted result.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
    public static IReadOnlyList<Measurement> Run(
        IReadOnlyList<ISortingAlgorithm> algorithms,
        BenchmarkOptions options,
        Action<Measurement>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Repetitions, 1);

        var budget = options.TimeBudget ?? BenchmarkOptions.DefaultTimeBudget;
        var active = new List<ISortingAlgorithm>(algorithms);
        var results = new List<Measurement>();

        WarmUp(active, options.Input);

        foreach (var size in options.Sizes)
        {
            if (active.Count == 0)
                break;

            var inputs = new int[options.Repetitions][];
            for (var r = 0; r < inputs.Length; r++)
                inputs[r] = InputGenerator.Create(options.Input, size, HashCode.Combine(options.Seed, size, r));

            foreach (var algorithm in active.ToArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var measurement = Measure(algorithm, inputs);
                results.Add(measurement);
                progress?.Invoke(measurement);
                if (measurement.Milliseconds > budget.TotalMilliseconds)
                    active.Remove(algorithm);
            }
        }

        return results;
    }

    private static Measurement Measure(ISortingAlgorithm algorithm, int[][] inputs)
    {
        var times = new double[inputs.Length];
        var stats = new SortStats[inputs.Length];
        var work = new int[inputs[0].Length];
        for (var r = 0; r < inputs.Length; r++)
        {
            inputs[r].CopyTo(work, 0);
            var start = Stopwatch.GetTimestamp();
            stats[r] = algorithm.Sort(work);
            times[r] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            if (!SortedSpan.IsSorted<int>(work))
                throw new UnsortedResultException(algorithm, work.Length);
        }

        // Median with Bojalib's O(n) selection; keep the stats of the same run.
        var order = new int[times.Length];
        for (var i = 0; i < order.Length; i++)
            order[i] = i;
        var mid = order.Length / 2;
        Selection.Select(order.AsSpan(), mid, (a, b) => times[a].CompareTo(times[b]));
        var median = order[mid];
        return new Measurement(algorithm, work.Length, times[median], stats[median]);
    }

    private static void WarmUp(IEnumerable<ISortingAlgorithm> algorithms, InputKind input)
    {
        var sample = InputGenerator.Create(input, 512, seed: 0);
        var work = new int[sample.Length];
        foreach (var algorithm in algorithms)
        {
            for (var i = 0; i < 3; i++)
            {
                sample.CopyTo(work, 0);
                algorithm.Sort(work);
            }
        }
    }
}
