using System;
using System.IO;
using Avalonia.Media.Imaging;
using iText.Kernel.Pdf;
using PDFtoImage;
using SkiaSharp;

namespace PDOff.Services;

public class PdfPageRenderService : IPdfPageRenderService
{
    public PageRenderResult RenderPage(string pdfPath, int zeroBasedPageIndex)
    {
        float pageWidth = 595f;
        float pageHeight = 842f;
        int pageCount = 1;

        try
        {
            // Step 1: Get page info with iText (fast, no rendering), then dispose fully
            using (var reader = new PdfReader(pdfPath))
            using (var pdfDoc = new PdfDocument(reader))
            {
                pageCount = pdfDoc.GetNumberOfPages();
                int clampedIdx = Math.Clamp(zeroBasedPageIndex, 0, pageCount - 1);
                var size = pdfDoc.GetPage(clampedIdx + 1).GetPageSize();
                pageWidth = size.GetWidth();
                pageHeight = size.GetHeight();
            }

            // Step 2: Render the page with PDFtoImage (iText is fully disposed above)
            int pageIdx = Math.Clamp(zeroBasedPageIndex, 0, pageCount - 1);
            using var stream = File.OpenRead(pdfPath);
            using var skBitmap = Conversion.ToImage(stream, page: (Index)pageIdx, options: new(Dpi: 150));

            using var encoded = skBitmap.Encode(SKEncodedImageFormat.Png, 90);
            using var ms = new MemoryStream(encoded.ToArray());
            var bitmap = new Bitmap(ms);

            return new PageRenderResult(bitmap, pageWidth, pageHeight, pageCount);
        }
        catch
        {
            return new PageRenderResult(null, pageWidth, pageHeight, pageCount);
        }
    }
}
