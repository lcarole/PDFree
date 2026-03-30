using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFree.Services;

namespace PDFree.ViewModels;

public partial class MergeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly IPdfMergeService _mergeService = new PdfMergeService();

    public ObservableCollection<string> Files { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    public MergeViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        Files.CollectionChanged += (_, _) => MergeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Instance["MergeDialogTitle"],
            AllowMultiple = true,
            FileTypeFilter = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (path is not null && !Files.Contains(path))
                Files.Add(path);
        }
    }

    [RelayCommand]
    private void RemoveFile(string path)
    {
        Files.Remove(path);
    }

    [RelayCommand]
    private void MoveUp(string path)
    {
        var index = Files.IndexOf(path);
        if (index > 0)
            Files.Move(index, index - 1);
    }

    [RelayCommand]
    private void MoveDown(string path)
    {
        var index = Files.IndexOf(path);
        if (index >= 0 && index < Files.Count - 1)
            Files.Move(index, index + 1);
    }

    private bool CanMerge() => Files.Count >= 2 && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanMerge))]
    private async Task Merge()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang.Instance["MergeSaveTitle"],
            DefaultExtension = "pdf",
            SuggestedFileName = Lang.Instance["MergeSuggestedName"],
            FileTypeChoices = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (file is null) return;
        var outputPath = file.TryGetLocalPath();
        if (outputPath is null) return;

        IsBusy = true;
        StatusMessage = null;

        try
        {
            var result = await Task.Run(() => _mergeService.Merge(Files.ToList(), outputPath));
            IsSuccess = result.Success;
            StatusMessage = result.Success
                ? string.Format(Lang.Instance["MergeSuccess"], Files.Count)
                : result.ErrorMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }
}
