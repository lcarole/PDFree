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
    private bool _isFrench = true;

    [ObservableProperty]
    private int _themeIndex; // 0=System, 1=Light, 2=Dark

    private readonly HomeViewModel _homeViewModel;

    public Lang Lang => Lang.Instance;

    public MainWindowViewModel()
    {
        _homeViewModel = new HomeViewModel(this);
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

    [RelayCommand]
    private void ToggleLanguage()
    {
        IsFrench = !IsFrench;
        Lang.Instance.SwitchLanguage(IsFrench ? "fr" : "en");
        // Rebuild the home tools so their titles/descriptions update
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
