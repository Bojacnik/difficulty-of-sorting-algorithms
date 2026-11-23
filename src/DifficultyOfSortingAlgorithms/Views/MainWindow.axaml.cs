using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DifficultyOfSortingAlgorithms.ViewModels;

namespace DifficultyOfSortingAlgorithms.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel model)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export results",
            SuggestedFileName = "sorting-benchmark.csv",
            DefaultExtension = "csv",
            FileTypeChoices = [new FilePickerFileType("CSV") { Patterns = ["*.csv"] }],
        });
        if (file is null)
            return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        model.ExportCsv(writer);
    }
}
