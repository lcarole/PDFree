using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDOff.Models;
using PDOff.Services;

namespace PDOff.ViewModels;

public partial class SignViewModel : ViewModelBase
{
    private readonly IPdfSignService _signService;
    private readonly IPdfPageRenderService _pageRenderService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignCommand))]
    private string? _selectedFile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignCommand))]
    private bool _hasSignature;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private SignPageTarget _pageTarget = SignPageTarget.LastPage;

    [ObservableProperty]
    private decimal _customPageNumber = 1;

    [ObservableProperty]
    private Bitmap? _pageBitmap;

    [ObservableProperty]
    private Bitmap? _signatureBitmap;

    [ObservableProperty]
    private bool _isLoadingPreview;

    private float _pdfPageWidth = 595f;
    private float _pdfPageHeight = 842f;

    // Normalized rect: X/Y/W/H in 0..1 range relative to the page
    // Y=0 is top of the page in screen space
    private Rect _normalizedSignatureRect = new Rect(0.6, 0.85, 0.3, 0.1);
    public Rect NormalizedSignatureRect
    {
        get => _normalizedSignatureRect;
        set
        {
            if (_normalizedSignatureRect == value) return;
            _normalizedSignatureRect = value;
            OnPropertyChanged();
        }
    }

    public Func<byte[]?>? GetSignatureImage { get; set; }
    public Action? ClearSignaturePad { get; set; }

    public SignViewModel(IPdfSignService signService, IPdfPageRenderService pageRenderService)
    {
        _signService = signService;
        _pageRenderService = pageRenderService;
    }

    partial void OnSelectedFileChanged(string? value)
    {
        if (value is not null)
            _ = LoadPagePreviewAsync();
    }

    partial void OnPageTargetChanged(SignPageTarget value)
    {
        if (SelectedFile is not null)
            _ = LoadPagePreviewAsync();
    }

    partial void OnCustomPageNumberChanged(decimal value)
    {
        if (PageTarget == SignPageTarget.SpecificPage && SelectedFile is not null)
            _ = LoadPagePreviewAsync();
    }

    private async Task LoadPagePreviewAsync()
    {
        if (SelectedFile is null) return;
        IsLoadingPreview = true;
        PageBitmap = null;
        try
        {
            var filePath = SelectedFile;
            var pageTargetSnapshot = PageTarget;
            var customPageSnapshot = (int)CustomPageNumber;

            var (bitmap, pw, ph, count) = await Task.Run(() =>
            {
                // Quick count first with iText, then dispose before PDFtoImage opens the file
                int cnt;
                using (var reader = new iText.Kernel.Pdf.PdfReader(filePath))
                using (var doc = new iText.Kernel.Pdf.PdfDocument(reader))
                    cnt = doc.GetNumberOfPages();

                int idx = pageTargetSnapshot switch
                {
                    SignPageTarget.FirstPage    => 0,
                    SignPageTarget.SpecificPage => Math.Clamp(customPageSnapshot - 1, 0, cnt - 1),
                    _                          => cnt - 1
                };

                var r = _pageRenderService.RenderPage(filePath, idx);
                return (r.Bitmap, r.PageWidth, r.PageHeight, r.PageCount);
            });

            _pdfPageWidth = pw;
            _pdfPageHeight = ph;
            PageBitmap = bitmap;
        }
        finally
        {
            IsLoadingPreview = false;
        }
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is null) return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Instance["SignDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (files.Count > 0)
            SelectedFile = files[0].TryGetLocalPath();
    }

    [RelayCommand]
    private void SetPageTarget(string targetStr)
    {
        PageTarget = Enum.Parse<SignPageTarget>(targetStr);
    }

    [RelayCommand]
    private void ClearSignature()
    {
        ClearSignaturePad?.Invoke();
        HasSignature = false;
        SignatureBitmap = null;
    }

    private bool CanSign() => SelectedFile is not null && HasSignature && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSign))]
    private async Task Sign()
    {
        if (SelectedFile is null) return;

        var signatureBytes = GetSignatureImage?.Invoke();
        if (signatureBytes is null) return;

        var storageProvider = GetStorageProvider();
        if (storageProvider is null)
        {
            IsSuccess = false;
            StatusMessage = Lang.Instance["StorageUnavailable"];
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang.Instance["SignSaveTitle"],
            DefaultExtension = "pdf",
            SuggestedFileName = Path.GetFileNameWithoutExtension(SelectedFile)
                + Lang.Instance["SignSuggestedSuffix"] + ".pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });

        if (file is null) return;
        var outputPath = file.TryGetLocalPath();
        if (outputPath is null) return;

        IsBusy = true;
        StatusMessage = null;

        try
        {
            var nr = NormalizedSignatureRect;
            // Convert normalized (screen, Y=0 top) to PDF coords (Y=0 bottom)
            float pdfX = (float)(nr.X * _pdfPageWidth);
            float pdfY = (float)((1.0 - nr.Y - nr.Height) * _pdfPageHeight);
            float pdfW = (float)(nr.Width * _pdfPageWidth);
            float pdfH = (float)(nr.Height * _pdfPageHeight);

            var options = new SignatureOptions(
                PageTarget: PageTarget,
                SpecificPage: (int)CustomPageNumber,
                X: pdfX,
                Y: pdfY,
                Width: pdfW,
                Height: pdfH);

            var result = await Task.Run(() => _signService.Sign(SelectedFile, outputPath, signatureBytes, options));

            IsSuccess = result.Success;
            StatusMessage = result.Success ? Lang.Instance["SignSuccess"] : result.ErrorMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
