using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DifficultyOfSortingAlgorithms.Core.Algorithms;
using DifficultyOfSortingAlgorithms.Core.Benchmarking;

namespace DifficultyOfSortingAlgorithms.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly List<Measurement> _results = [];
    private readonly Dictionary<ISortingAlgorithm, ChartSeries> _seriesByAlgorithm = [];
    private CancellationTokenSource? _cancellation;

    public MainViewModel()
    {
        foreach (var algorithm in AlgorithmCatalog.Original)
            OriginalAlgorithms.Add(new AlgorithmOption(algorithm, isSelected: true));
        foreach (var algorithm in AlgorithmCatalog.Bojalib)
            BojalibAlgorithms.Add(new AlgorithmOption(algorithm, isSelected: false));
    }

    public ObservableCollection<AlgorithmOption> OriginalAlgorithms { get; } = [];
    public ObservableCollection<AlgorithmOption> BojalibAlgorithms { get; } = [];
    public ObservableCollection<ChartSeries> Series { get; } = [];

    public IReadOnlyList<InputKind> InputKinds { get; } = Enum.GetValues<InputKind>();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Bound from XAML.")]
    public decimal MaxSteps => SizeSeries.MaxSteps;

    /// <summary>Number of sizes: 100·i² for i = 1 … Steps (the original "Iterations" field).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LargestSizeText))]
    private decimal? _steps = 13;

    [ObservableProperty]
    private InputKind _input = InputKind.Random;

    [ObservableProperty]
    private decimal? _repetitions = 3;

    [ObservableProperty]
    private decimal? _timeBudgetSeconds = 2;

    [ObservableProperty]
    private bool _logScale = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand), nameof(CancelCommand))]
    private bool _isRunning;

    [ObservableProperty]
    private string _status = "Pick algorithms and press Run.";

    /// <summary>Bumped whenever a series gains a point, so the chart knows to redraw.</summary>
    [ObservableProperty]
    private int _chartRevision;

    public bool HasResults => _results.Count > 0;

    public string LargestSizeText
    {
        get
        {
            var steps = (int)Math.Clamp(Steps ?? 1, 1, SizeSeries.MaxSteps);
            return $"Largest array: {100L * steps * steps:N0} elements";
        }
    }

    private bool CanRun() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        var algorithms = OriginalAlgorithms.Concat(BojalibAlgorithms).Where(o => o.IsSelected).ToList();
        if (algorithms.Count == 0)
        {
            Status = "Select at least one algorithm.";
            return;
        }

        _results.Clear();
        _seriesByAlgorithm.Clear();
        Series.Clear();
        foreach (var option in algorithms)
        {
            var series = new ChartSeries(option.Algorithm);
            _seriesByAlgorithm[option.Algorithm] = series;
            Series.Add(series);
        }

        ChartRevision++;
        OnPropertyChanged(nameof(HasResults));

        var options = new BenchmarkOptions(
            SizeSeries.Quadratic((int)Math.Clamp(Steps ?? 1, 1, SizeSeries.MaxSteps)),
            Input,
            (int)Math.Clamp(Repetitions ?? 1, 1, 100),
            TimeSpan.FromSeconds((double)Math.Clamp(TimeBudgetSeconds ?? 2, 0.01m, 3600)));

        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;
        IsRunning = true;
        Status = "Warming up…";
        try
        {
            var selected = algorithms.Select(o => o.Algorithm).ToList();
            await Task.Run(
                () => SortBenchmark.Run(selected, options, m => Dispatcher.UIThread.Post(() => Add(m)), token),
                token);
            // Let the measurements still queued on the UI thread land before the final status.
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            Status = $"Done: {_results.Count} measurements. Algorithms that exceeded the time budget stopped early.";
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            Status = $"Cancelled after {_results.Count} measurements.";
        }
        catch (SortBenchmark.UnsortedResultException e)
        {
            Status = "Error: " + e.Message;
        }
        finally
        {
            IsRunning = false;
            _cancellation.Dispose();
            _cancellation = null;
        }
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void Cancel() => _cancellation?.Cancel();

    [RelayCommand]
    private void SelectAll(string group) => SetGroup(group, true);

    [RelayCommand]
    private void SelectNone(string group) => SetGroup(group, false);

    partial void OnLogScaleChanged(bool value) => ChartRevision++;

    public void Dispose() => _cancellation?.Cancel();

    /// <summary>Writes the results as CSV, which Excel opens directly.</summary>
    public void ExportCsv(TextWriter writer) => CsvExport.Write(writer, _results);

    private void SetGroup(string group, bool selected)
    {
        foreach (var option in group == "Bojalib" ? BojalibAlgorithms : OriginalAlgorithms)
            option.IsSelected = selected;
    }

    private void Add(Measurement measurement)
    {
        _results.Add(measurement);
        _seriesByAlgorithm[measurement.Algorithm].Measurements.Add(measurement);
        ChartRevision++;
        if (_results.Count == 1)
            OnPropertyChanged(nameof(HasResults));

        var ops = measurement.Stats == SortStats.None
            ? ""
            : $", {measurement.Stats.Comparisons:N0} comparisons";
        Status = $"{measurement.Algorithm.Name}: {measurement.Size:N0} elements in {measurement.Milliseconds:0.###} ms{ops}";
    }
}
