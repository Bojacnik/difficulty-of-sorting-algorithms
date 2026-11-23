using CsCheck;
using DifficultyOfSortingAlgorithms.Core.Algorithms;

namespace DifficultyOfSortingAlgorithms.Tests;

public sealed class AlgorithmTests
{
    private static IEnumerable<ISortingAlgorithm> All => AlgorithmCatalog.All;

    [TestCaseSource(nameof(All))]
    public void Sort_AnyArray_MatchesArraySort(ISortingAlgorithm algorithm) =>
        Gen.Int[-1000, 1000].Array[0, 300].Sample(input =>
        {
            var expected = (int[])input.Clone();
            Array.Sort(expected);
            var actual = (int[])input.Clone();
            algorithm.Sort(actual);
            actual.Should().Equal(expected);
        });

    [TestCaseSource(nameof(All))]
    public void Sort_EdgeCases_Sorted(ISortingAlgorithm algorithm)
    {
        int[][] cases =
        [
            [],
            [1],
            [2, 1],
            [5, 5, 5, 5],
            [int.MaxValue, int.MinValue, 0, -1, 1],
            [9, 1, 2, 3, 4, 5, 6, 7, 8, 0],   // max first and min last: the min-max swap trap
            [.. Enumerable.Range(0, 1000).Reverse()],
        ];
        foreach (var input in cases)
        {
            var actual = (int[])input.Clone();
            algorithm.Sort(actual);
            actual.Should().BeInAscendingOrder();
            actual.Should().BeEquivalentTo(input);
        }
    }

    [Test]
    public void BubbleSort_AlwaysDoesAllPasses()
    {
        var data = Enumerable.Range(0, 100).ToArray();
        var stats = new BubbleSort().Sort(data);
        stats.Comparisons.Should().Be(99L * 99);
        stats.Writes.Should().Be(0);
    }

    [Test]
    public void ImprovedBubbleSort_SortedInput_StopsAfterOnePass()
    {
        var data = Enumerable.Range(0, 100).ToArray();
        new ImprovedBubbleSort().Sort(data).Comparisons.Should().Be(99);
    }

    [Test]
    public void ImprovedBubbleSort_ReversedInput_DoesTriangularWork()
    {
        var data = Enumerable.Range(0, 100).Reverse().ToArray();
        var stats = new ImprovedBubbleSort().Sort(data);
        stats.Comparisons.Should().Be(99L * 100 / 2);
        stats.Writes.Should().Be(2L * 99 * 100 / 2);
    }

    [Test]
    public void InsertionSort_SortedInput_IsLinear()
    {
        var data = Enumerable.Range(0, 100).ToArray();
        new InsertionSort().Sort(data).Comparisons.Should().Be(99);
    }

    [Test]
    public void MergeSort_Comparisons_AreAtMostNLogN()
    {
        var data = Enumerable.Range(0, 1024).Reverse().ToArray();
        new MergeSort().Sort(data).Comparisons.Should().BeLessThanOrEqualTo(1024 * 10);
    }

    [Test]
    public void BojalibAlgorithms_DoNotCount()
    {
        foreach (var algorithm in AlgorithmCatalog.Bojalib)
            algorithm.Sort([3, 1, 2]).Should().Be(SortStats.None);
    }
}
