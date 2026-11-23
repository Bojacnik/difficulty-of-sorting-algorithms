using System.Globalization;

namespace DifficultyOfSortingAlgorithms.Core.Benchmarking;

/// <summary>Writes results as CSV (one row per size, one column per algorithm, times in ms). Excel opens it directly.</summary>
public static class CsvExport
{
    /// <summary>Writes <paramref name="measurements"/> to <paramref name="writer"/>.</summary>
    public static void Write(TextWriter writer, IEnumerable<Measurement> measurements)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(measurements);

        var list = measurements.ToList();
        var algorithms = list.Select(m => m.Algorithm).Distinct().ToList();
        var bySize = list.ToLookup(m => m.Size);

        writer.Write("Size");
        foreach (var algorithm in algorithms)
            writer.Write("," + Escape(algorithm.Name + " (ms)"));
        writer.WriteLine();

        foreach (var size in list.Select(m => m.Size).Distinct().Order())
        {
            writer.Write(size.ToString(CultureInfo.InvariantCulture));
            foreach (var algorithm in algorithms)
            {
                var hit = bySize[size].FirstOrDefault(m => m.Algorithm == algorithm);
                writer.Write(',');
                if (hit is not null)
                    writer.Write(hit.Milliseconds.ToString("0.###", CultureInfo.InvariantCulture));
            }

            writer.WriteLine();
        }
    }

    private static string Escape(string value) =>
        value.AsSpan().IndexOfAny(",\"\n") < 0 ? value : "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
