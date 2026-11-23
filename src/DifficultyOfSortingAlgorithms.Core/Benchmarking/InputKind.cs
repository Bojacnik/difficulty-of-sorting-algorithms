namespace DifficultyOfSortingAlgorithms.Core.Benchmarking;

/// <summary>The shape of the arrays to sort.</summary>
public enum InputKind
{
    /// <summary>Uniformly random values in [0, 50 000), like the original app.</summary>
    Random,

    /// <summary>Already sorted ascending.</summary>
    Sorted,

    /// <summary>Sorted descending: the worst case for bubble and insertion sort.</summary>
    Reversed,

    /// <summary>Sorted, then 1% of the elements swapped at random.</summary>
    NearlySorted,

    /// <summary>Random values drawn from only 16 distinct keys.</summary>
    FewUnique,
}

/// <summary>Creates benchmark input arrays. The same seed always gives the same array.</summary>
public static class InputGenerator
{
    /// <summary>Upper bound (exclusive) of the random values, as in the original app.</summary>
    public const int MaxValue = 50_000;

    /// <summary>Creates an array of <paramref name="size"/> elements of the given <paramref name="kind"/>.</summary>
    public static int[] Create(InputKind kind, int size, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        var random = new Random(seed);
        var data = new int[size];
        switch (kind)
        {
            case InputKind.Random:
                for (var i = 0; i < size; i++)
                    data[i] = random.Next(MaxValue);
                break;
            case InputKind.Sorted:
                FillAscending(data);
                break;
            case InputKind.Reversed:
                FillAscending(data);
                data.AsSpan().Reverse();
                break;
            case InputKind.NearlySorted:
                FillAscending(data);
                for (var swaps = size / 100; swaps > 0; swaps--)
                {
                    int a = random.Next(size), b = random.Next(size);
                    (data[a], data[b]) = (data[b], data[a]);
                }

                break;
            case InputKind.FewUnique:
                for (var i = 0; i < size; i++)
                    data[i] = random.Next(16) * (MaxValue / 16);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        return data;
    }

    private static void FillAscending(int[] data)
    {
        // Spread over the same value range as random input.
        for (var i = 0; i < data.Length; i++)
            data[i] = (int)((long)i * MaxValue / Math.Max(1, data.Length));
    }
}
