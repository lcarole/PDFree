using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDOff.Models;

namespace PDOff.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly IServiceProvider _services;

    public ObservableCollection<ToolItem> Tools { get; } = new();

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _appTitle = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _appSubtitle = string.Empty;

    public HomeViewModel(MainWindowViewModel mainVm, IServiceProvider services)
    {
        _mainVm = mainVm;
        _services = services;
        RefreshTools();
    }

    public void RefreshTools()
    {
        AppTitle = Lang["AppTitle"];
        AppSubtitle = Lang["AppSubtitle"];

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

        Tools.Add(new("rotate",
            Lang["ToolRotateTitle"],
            Lang["ToolRotateDesc"],
            StreamGeometry.Parse("M7.11 8.53L5.7 7.11C4.8 8.27 4.24 9.61 4.07 11h2.02c.14-1.01.58-1.96 1.02-2.47zM6.09 13H4.07c.17 1.39.72 2.73 1.62 3.89l1.41-1.42c-.44-.51-.87-1.46-1.01-2.47zm1.01 5.32c1.16.9 2.51 1.44 3.9 1.61V17.9c-1-.14-1.95-.58-2.46-1.02L7.1 18.32zM13 4.07V1L8.45 5.55 13 10V6.09c2.84.48 5 2.94 5 5.91s-2.16 5.43-5 5.91v2.02c3.95-.49 7-3.85 7-7.93s-3.05-7.44-7-7.93z"),
            Color.Parse("#9C27B0")));

        Tools.Add(new("sign",
            Lang["ToolSignTitle"],
            Lang["ToolSignDesc"],
            StreamGeometry.Parse("M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 000-1.41l-2.34-2.34a1 1 0 00-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"),
            Color.Parse("#607D8B")));
    }

    [RelayCommand]
    private void OpenTool(string toolId)
    {
        ViewModelBase vm = toolId switch
        {
            "merge" => _services.GetRequiredService<MergeViewModel>(),
            "split" => _services.GetRequiredService<SplitViewModel>(),
            "compress" => _services.GetRequiredService<CompressViewModel>(),
            "rotate" => _services.GetRequiredService<RotateViewModel>(),
            "sign" => _services.GetRequiredService<SignViewModel>(),
            _ => this
        };
        _mainVm.NavigateTo(vm);
    }
}
