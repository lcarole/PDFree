using PDFree.Models;

namespace PDFree.Services;

public enum CompressionLevel
{
    Low,
    Medium,
    High
}

public interface IPdfCompressService
{
    PdfToolResult Compress(string inputPath, string outputPath, CompressionLevel level);
}
