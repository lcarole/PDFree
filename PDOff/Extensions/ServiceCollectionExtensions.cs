using Microsoft.Extensions.DependencyInjection;
using PDOff.Services;
using PDOff.ViewModels;

namespace PDOff.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddSingleton<IPdfMergeService, PdfMergeService>();
        collection.AddSingleton<IPdfSplitService, PdfSplitService>();
        collection.AddSingleton<IPdfCompressService, PdfCompressService>();
        collection.AddSingleton<IPdfRotateService, PdfRotateService>();
        collection.AddSingleton<IPdfSignService, PdfSignService>();
        collection.AddSingleton<IPdfPageRenderService, PdfPageRenderService>();
    }

    public static void AddViewModels(this IServiceCollection collection)
    {
        collection.AddTransient<MergeViewModel>();
        collection.AddTransient<SplitViewModel>();
        collection.AddTransient<CompressViewModel>();
        collection.AddTransient<RotateViewModel>();
        collection.AddTransient<SignViewModel>();
        collection.AddSingleton<MainWindowViewModel>();
    }
}