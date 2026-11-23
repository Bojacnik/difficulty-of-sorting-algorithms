using DifficultyOfSortingAlgorithms.Core.Algorithms;
using DifficultyOfSortingAlgorithms.Core.Benchmarking;

namespace DifficultyOfSortingAlgorithms.Tests;

public sealed class BenchmarkTests
{
    [Test]
    public void Quadratic_MatchesOriginalSeries() =>
        SizeSeries.Quadratic(4).Should().Equal(100, 400, 900, 1600);

    [TestCase(0)]
    [TestCase(SizeSeries.MaxSteps + 1)]
    public void Quadratic_OutOfRange_Throws(int steps) =>
        FluentActions.Invoking(() => SizeSeries.Quadratic(steps)).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Create_SameSeed_SameArray([Values] InputKind kind) =>
        InputGenerator.Create(kind, 500, 7).Should().Equal(InputGenerator.Create(kind, 500, 7));

    [Test]
    public void Create_SortedAndReversed_HaveTheirShape()
    {
        InputGenerator.Create(InputKind.Sorted, 500, 1).Should().BeInAscendingOrder();
        InputGenerator.Create(InputKind.Reversed, 500, 1).Should().BeInDescendingOrder();
    }

    [Test]
    public void Run_EveryAlgorithmSeesTheSameInput()
    {
        var a = new RecordingAlgorithm("a");
        var b = new RecordingAlgorithm("b");
        SortBenchmark.Run([a, b], new BenchmarkOptions([300], Repetitions: 1));
        a.Inputs[^1].Should().Equal(b.Inputs[^1]);
    }

    [Test]
    public void Run_ReportsEverySizeAndAlgorithm()
    {
        var reported = new List<Measurement>();
        var results = SortBenchmark.Run(AlgorithmCatalog.Original, new BenchmarkOptions([100, 400]), reported.Add);
        results.Should().HaveCount(10);
        reported.Should().Equal(results);
        results.Should().OnlyContain(m => m.Milliseconds >= 0 && m.Stats.Comparisons > 0);
    }

    [Test]
    public void Run_SlowAlgorithm_DropsOutAfterExceedingBudget()
    {
        var slow = new SlowAlgorithm();
        var results = SortBenchmark.Run(
            [slow, new InsertionSort()],
            new BenchmarkOptions([10, 20, 30], Repetitions: 1, TimeBudget: TimeSpan.FromMilliseconds(1)));
        results.Count(m => m.Algorithm == slow).Should().Be(1);
        results.Count(m => m.Algorithm is InsertionSort).Should().Be(3);
    }

    [Test]
    public void Run_BrokenAlgorithm_Throws() =>
        FluentActions.Invoking(() => SortBenchmark.Run([new BrokenAlgorithm()], new BenchmarkOptions([100])))
            .Should().Throw<SortBenchmark.UnsortedResultException>();

    [Test]
    public void Run_Cancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        FluentActions.Invoking(() => SortBenchmark.Run(AlgorithmCatalog.Original, new BenchmarkOptions([100]), null, cts.Token))
            .Should().Throw<OperationCanceledException>();
    }

    [Test]
    public void CsvExport_WritesOneRowPerSize()
    {
        var a = new InsertionSort();
        var b = new MergeSort();
        Measurement[] data =
        [
            new(a, 100, 1.5, default), new(b, 100, 0.25, default), new(b, 400, 2, default),
        ];
        using var writer = new StringWriter { NewLine = "\n" };
        CsvExport.Write(writer, data);
        writer.ToString().Should().Be(
            "Size,Insertion sort (ms),Merge sort (ms)\n" +
            "100,1.5,0.25\n" +
            "400,,2\n");
    }

    private sealed class RecordingAlgorithm(string name) : ISortingAlgorithm
    {
        public List<int[]> Inputs { get; } = [];
        public string Name => name;
        public AlgorithmOrigin Origin => AlgorithmOrigin.Original;
        public string Complexity => "";
        public string Description => "";

        public SortStats Sort(Span<int> data)
        {
            Inputs.Add(data.ToArray());
            data.Sort();
            return default;
        }
    }

    private sealed class SlowAlgorithm : ISortingAlgorithm
    {
        public string Name => "slow";
        public AlgorithmOrigin Origin => AlgorithmOrigin.Original;
        public string Complexity => "";
        public string Description => "";

        public SortStats Sort(Span<int> data)
        {
            Thread.Sleep(5);
            data.Sort();
            return default;
        }
    }

    private sealed class BrokenAlgorithm : ISortingAlgorithm
    {
        public string Name => "broken";
        public AlgorithmOrigin Origin => AlgorithmOrigin.Original;
        public string Complexity => "";
        public string Description => "";
        public SortStats Sort(Span<int> data) => default;
    }
}
