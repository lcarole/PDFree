using System;
using iText.Kernel.Pdf;
using PDFree.Models;

namespace PDFree.Services;

public class PdfCompressService : IPdfCompressService
{
    public PdfToolResult Compress(string inputPath, string outputPath, CompressionLevel level)
    {
        try
        {
            var writerProperties = new WriterProperties();

            switch (level)
            {
                case CompressionLevel.Low:
                    writerProperties.SetCompressionLevel(CompressionConstants.DEFAULT_COMPRESSION);
                    break;
                case CompressionLevel.Medium:
                    writerProperties.SetCompressionLevel(CompressionConstants.BEST_SPEED);
                    writerProperties.SetFullCompressionMode(true);
                    break;
                case CompressionLevel.High:
                    writerProperties.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                    writerProperties.SetFullCompressionMode(true);
                    break;
            }

            using var reader = new PdfReader(inputPath);
            using var writer = new PdfWriter(outputPath, writerProperties);
            using var srcDoc = new PdfDocument(reader);
            using var destDoc = new PdfDocument(writer);

            srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), destDoc);

            // Remove unused objects and metadata for higher compression
            if (level >= CompressionLevel.Medium)
            {
                var info = destDoc.GetDocumentInfo();
                info.SetCreator("");
                info.SetProducer("");
            }

            return new PdfToolResult(true, outputPath);
        }
        catch (Exception ex)
        {
            return new PdfToolResult(false, ErrorMessage: string.Format(Lang.Instance["CompressError"], ex.Message));
        }
    }
}
