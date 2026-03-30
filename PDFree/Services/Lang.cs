using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace PDFree.Services;

/// <summary>
/// Provides localized strings from ResX resources. Singleton accessible from AXAML via x:Static.
/// Implements INotifyPropertyChanged so the UI refreshes when the language changes.
/// </summary>
public sealed class Lang : INotifyPropertyChanged
{
    private static readonly Lazy<Lang> _instance = new(() => new Lang());
    public static Lang Instance => _instance.Value;

    private readonly ResourceManager _rm = new("PDFree.Resources.Strings", typeof(Lang).Assembly);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Indexer used by AXAML bindings: {Binding [Key], Source={x:Static services:Lang.Instance}}
    /// </summary>
    public string this[string key] => _rm.GetString(key, Thread.CurrentThread.CurrentUICulture) ?? $"[{key}]";

    public string CurrentLanguage => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

    public void SwitchLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;

        // Notify ALL bindings that use the indexer to re-evaluate
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }
}
