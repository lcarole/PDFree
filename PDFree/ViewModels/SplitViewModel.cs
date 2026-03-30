using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFree.Services;

namespace PDFree.ViewModels;

public partial class SplitViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly IPdfSplitService _splitService = new PdfSplitService();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SplitCommand))]
    private string? _selectedFile;

    [ObservableProperty]
    private SplitMode _splitMode = SplitMode.EachPage;

    [ObservableProperty]
    private int _everyNPages = 2;

    [ObservableProperty]
    private string _pageRange = "1-3";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SplitCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    public SplitViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Instance["SplitDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (files.Count > 0)
            SelectedFile = files[0].TryGetLocalPath();
    }

    private bool CanSplit() => SelectedFile is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSplit))]
    private async Task Split()
    {
        if (SelectedFile is null) return;

        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Lang.Instance["SplitOutputFolder"],
            AllowMultiple = false
        });

        if (folders.Count == 0) return;
        var outputDir = folders[0].TryGetLocalPath();
        if (outputDir is null) return;

        IsBusy = true;
        StatusMessage = null;

        try
        {
            var result = await Task.Run(() => _splitService.Split(
                SelectedFile, outputDir, SplitMode, EveryNPages, PageRange));

            IsSuccess = result.Success;
            StatusMessage = result.Success
                ? string.Format(Lang.Instance["SplitSuccess"], outputDir)
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
