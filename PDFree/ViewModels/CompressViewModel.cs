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

public partial class CompressViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly IPdfCompressService _compressService = new PdfCompressService();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompressCommand))]
    private string? _selectedFile;

    [ObservableProperty]
    private CompressionLevel _compressionLevel = CompressionLevel.Medium;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompressCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private long _originalSize;

    [ObservableProperty]
    private long _compressedSize;

    public CompressViewModel(MainWindowViewModel mainVm)
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
            Title = Lang.Instance["CompressDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (files.Count > 0)
        {
            SelectedFile = files[0].TryGetLocalPath();
            if (SelectedFile is not null)
                OriginalSize = new FileInfo(SelectedFile).Length;
        }
    }

    private bool CanCompress() => SelectedFile is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanCompress))]
    private async Task Compress()
    {
        if (SelectedFile is null) return;

        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang.Instance["CompressSaveTitle"],
            DefaultExtension = "pdf",
            SuggestedFileName = Path.GetFileNameWithoutExtension(SelectedFile) + Lang.Instance["CompressSuggestedSuffix"] + ".pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (file is null) return;
        var outputPath = file.TryGetLocalPath();
        if (outputPath is null) return;

        IsBusy = true;
        StatusMessage = null;

        try
        {
            var result = await Task.Run(() => _compressService.Compress(SelectedFile, outputPath, CompressionLevel));

            IsSuccess = result.Success;
            if (result.Success && File.Exists(outputPath))
            {
                CompressedSize = new FileInfo(outputPath).Length;
                var ratio = OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100 : 0;
                StatusMessage = string.Format(Lang.Instance["CompressSuccess"], ratio);
            }
            else
            {
                StatusMessage = result.ErrorMessage;
            }
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
