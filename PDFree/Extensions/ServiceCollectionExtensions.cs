using Microsoft.Extensions.DependencyInjection;
using PDFree.Services;
using PDFree.ViewModels;

namespace PDFree.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddSingleton<IPdfMergeService, PdfMergeService>();
        collection.AddSingleton<IPdfSplitService, PdfSplitService>();
        collection.AddSingleton<IPdfCompressService, PdfCompressService>();
    }

    public static void AddViewModels(this IServiceCollection collection)
    {
        collection.AddTransient<MergeViewModel>();
        collection.AddTransient<SplitViewModel>();
        collection.AddTransient<CompressViewModel>();
        collection.AddSingleton<MainWindowViewModel>();
    }
}