using Avalonia.Media.Imaging;

namespace PDOff.Services;

public record PageRenderResult(Bitmap? Bitmap, float PageWidth, float PageHeight, int PageCount);

public interface IPdfPageRenderService
{
    PageRenderResult RenderPage(string pdfPath, int zeroBasedPageIndex);
}
