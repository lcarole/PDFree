using System.Collections.Generic;
using PDFree.Models;

namespace PDFree.Services;

public interface IPdfMergeService
{
    PdfToolResult Merge(IReadOnlyList<string> inputPaths, string outputPath);
}
