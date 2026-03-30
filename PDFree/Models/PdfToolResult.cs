namespace PDFree.Models;

public record PdfToolResult(bool Success, string? OutputPath = null, string? ErrorMessage = null);
