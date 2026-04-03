using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PDOff.ViewModels;
using PDOff.Views;

namespace PDOff;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> ViewMap = new()
    {
        [typeof(HomeViewModel)] = () => new HomeView(),
        [typeof(MergeViewModel)] = () => new MergeView(),
        [typeof(SplitViewModel)] = () => new Views.SplitView(),
        [typeof(CompressViewModel)] = () => new CompressView(),
        [typeof(RotateViewModel)] = () => new RotateView(),
        [typeof(SignViewModel)] = () => new SignView(),
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (ViewMap.TryGetValue(param.GetType(), out var factory))
            return factory();

        return new TextBlock { Text = "Not Found: " + param.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
