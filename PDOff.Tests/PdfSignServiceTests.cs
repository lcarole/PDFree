using iText.Kernel.Pdf;
using PDOff.Models;
using PDOff.Services;
using PDOff.Tests.Helpers;

namespace PDOff.Tests;

public class PdfSignServiceTests : IDisposable
{
    private readonly PdfSignService _service = new();
    private readonly string _tempDir;

    public PdfSignServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"PDOff_SignTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    /// <summary>Minimal 1x1 black pixel PNG valid for iText ImageDataFactory.</summary>
    private static byte[] CreateTestSignatureImage()
    {
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC,
            0x33, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
            0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }

    [Fact]
    public void Sign_NonExistentPdf_ReturnsError()
    {
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign("/nonexistent/file.pdf", output, image, new SignatureOptions());

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Sign_ValidInputs_ProducesSignedPdf()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 2);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign(input, output, image, new SignatureOptions());

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.Equal(output, result.OutputPath);
        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length > 0);
    }

    [Fact]
    public void Sign_OutputPdf_IsReadable()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 3);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign(input, output, image, new SignatureOptions());

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");

        using var reader = new PdfReader(output);
        using var doc = new PdfDocument(reader);
        Assert.Equal(3, doc.GetNumberOfPages());
    }

    [Fact]
    public void Sign_OutputPdf_PreservesPageCount()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 5);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign(input, output, image, new SignatureOptions());

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");

        using var reader = new PdfReader(output);
        using var doc = new PdfDocument(reader);
        Assert.Equal(5, doc.GetNumberOfPages());
    }

    [Fact]
    public void Sign_EmptyImageArray_ReturnsError()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir);
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign(input, output, Array.Empty<byte>(), new SignatureOptions());

        Assert.False(result.Success);
        Assert.False(File.Exists(output));
    }

    [Fact]
    public void Sign_SinglePagePdf_Succeeds()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 1);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed.pdf");

        var result = _service.Sign(input, output, image, new SignatureOptions());

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }

    [Fact]
    public void Sign_TopLeftPosition_Succeeds()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 2);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed_topleft.pdf");

        var result = _service.Sign(input, output, image,
            new SignatureOptions(Position: SignPosition.TopLeft));

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }

    [Fact]
    public void Sign_FirstPageTarget_Succeeds()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 3);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed_firstpage.pdf");

        var result = _service.Sign(input, output, image,
            new SignatureOptions(PageTarget: SignPageTarget.FirstPage));

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }

    [Fact]
    public void Sign_SpecificPage_Succeeds()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 5);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed_page3.pdf");

        var result = _service.Sign(input, output, image,
            new SignatureOptions(PageTarget: SignPageTarget.SpecificPage, SpecificPage: 3));

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }

    [Fact]
    public void Sign_SpecificPageOutOfRange_ClampsToLastPage()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 2);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed_clamped.pdf");

        var result = _service.Sign(input, output, image,
            new SignatureOptions(PageTarget: SignPageTarget.SpecificPage, SpecificPage: 999));

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }

    [Fact]
    public void Sign_CustomSize_Succeeds()
    {
        var input = PdfTestHelper.CreateTestPdf(_tempDir, pageCount: 1);
        var image = CreateTestSignatureImage();
        var output = Path.Combine(_tempDir, "signed_customsize.pdf");

        var result = _service.Sign(input, output, image,
            new SignatureOptions(MaxWidth: 100f, MaxHeight: 40f));

        Assert.True(result.Success, result.ErrorMessage ?? "Sign failed");
        Assert.True(File.Exists(output));
    }
}
