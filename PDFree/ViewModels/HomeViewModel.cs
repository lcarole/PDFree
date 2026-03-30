using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PDFree.Models;
using PDFree.Services;

namespace PDFree.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;

    public ObservableCollection<ToolItem> Tools { get; } = new();

    public Lang Lang => Lang.Instance;

    public HomeViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        RefreshTools();
    }

    public void RefreshTools()
    {
        Tools.Clear();
        Tools.Add(new("merge",
            Lang["ToolMergeTitle"],
            Lang["ToolMergeDesc"],
            StreamGeometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm5 11h-4v4h-2v-4H7v-2h4V7h2v4h4v2z"),
            Color.Parse("#4CAF50")));

        Tools.Add(new("split",
            Lang["ToolSplitTitle"],
            Lang["ToolSplitDesc"],
            StreamGeometry.Parse("M14 4l2.29 2.29-2.88 2.88 1.42 1.42 2.88-2.88L20 10V4h-6zm-4 0H4v6l2.29-2.29 4.71 4.7V20h2v-8.41l-5.29-5.3L10 4z"),
            Color.Parse("#2196F3")));

        Tools.Add(new("compress",
            Lang["ToolCompressTitle"],
            Lang["ToolCompressDesc"],
            StreamGeometry.Parse("M8 11h3v10h2V11h3l-4-4-4 4zM4 3v2h16V3H4z"),
            Color.Parse("#FF9800")));
    }

    [RelayCommand]
    private void OpenTool(string toolId)
    {
        ViewModelBase vm = toolId switch
        {
            "merge" => new MergeViewModel(_mainVm),
            "split" => new SplitViewModel(_mainVm),
            "compress" => new CompressViewModel(_mainVm),
            _ => this
        };
        _mainVm.NavigateTo(vm);
    }
}
