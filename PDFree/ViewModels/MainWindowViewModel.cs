using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFree.Services;

namespace PDFree.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private int _languageIndex; // 0=FR, 1=EN

    [ObservableProperty]
    private int _themeIndex; // 0=System, 1=Light, 2=Dark

    private readonly HomeViewModel _homeViewModel;

    public Lang Lang => Lang.Instance;

    public MainWindowViewModel(IServiceProvider services)
    {
        _homeViewModel = new HomeViewModel(this, services);
        _currentView = _homeViewModel;
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentView = viewModel;
        CanGoBack = viewModel != _homeViewModel;
    }

    [RelayCommand]
    private void GoHome()
    {
        NavigateTo(_homeViewModel);
    }

    partial void OnLanguageIndexChanged(int value)
    {
        var code = value switch { 1 => "en", _ => "fr" };
        Lang.Instance.SwitchLanguage(code);
        _homeViewModel.RefreshTools();
    }

    partial void OnThemeIndexChanged(int value)
    {
        if (Application.Current is not { } app) return;

        app.RequestedThemeVariant = value switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
