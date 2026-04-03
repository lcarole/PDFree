using System;
using System.IO;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using PDOff.Models;

namespace PDOff.Services;

public class PdfSignService : IPdfSignService
{
    public PdfToolResult Sign(string inputPath, string outputPath, byte[] signatureImage, SignatureOptions options)
    {
        if (!File.Exists(inputPath))
            return new PdfToolResult(false, ErrorMessage: string.Format(Lang.Instance["FileNotFound"], inputPath));

        if (signatureImage.Length == 0)
            return new PdfToolResult(false, ErrorMessage: string.Format(Lang.Instance["SignError"], "empty image"));

        try
        {
            using var reader = new PdfReader(inputPath);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);
            using var document = new Document(pdfDoc);

            int pageNum = options.PageTarget switch
            {
                SignPageTarget.FirstPage    => 1,
                SignPageTarget.SpecificPage => Math.Clamp(options.SpecificPage, 1, pdfDoc.GetNumberOfPages()),
                _                          => pdfDoc.GetNumberOfPages()
            };

            var imageData = ImageDataFactory.Create(signatureImage);
            var image = new Image(imageData);
            image.ScaleToFit(options.Width, options.Height);
            image.SetFixedPosition(pageNum, options.X, options.Y);
            document.Add(image);

            return new PdfToolResult(true, outputPath);
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return new PdfToolResult(false, ErrorMessage: string.Format(Lang.Instance["SignError"], ex.Message));
        }
    }

    private static void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
