using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using PDOff.Services;

namespace PDOff.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Exposes Lang as an instance property so compiled AXAML bindings can subscribe
    /// to its PropertyChanged("Item[]") notifications when language switches.
    /// </summary>
    public Lang Lang => Lang.Instance;

    protected static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }
}
