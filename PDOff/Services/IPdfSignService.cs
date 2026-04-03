using PDOff.Models;

namespace PDOff.Services;

public interface IPdfSignService
{
    PdfToolResult Sign(string inputPath, string outputPath, byte[] signatureImage, SignatureOptions options);
}
