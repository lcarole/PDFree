using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PDOff.Controls;
using PDOff.ViewModels;

namespace PDOff.Views;

public partial class SignView : UserControl
{
    public SignView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);

        SignaturePad.PropertyChanged += (_, args) =>
        {
            if (args.Property != SignaturePadControl.HasSignatureProperty) return;
            if (DataContext is not SignViewModel vm) return;

            vm.HasSignature = (bool)args.NewValue!;

            if (vm.HasSignature)
            {
                var bytes = SignaturePad.ExportToPng();
                if (bytes is not null)
                {
                    using var ms = new MemoryStream(bytes);
                    vm.SignatureBitmap = new Bitmap(ms);
                }
            }
            else
            {
                vm.SignatureBitmap = null;
            }
        };

        DataContextChanged += (_, _) =>
        {
            if (DataContext is SignViewModel vm)
            {
                vm.GetSignatureImage = () => SignaturePad.ExportToPng();
                vm.ClearSignaturePad = () => SignaturePad.Clear();
                vm.HasSignature = SignaturePad.HasSignature;
            }
        };
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
            Classes.Add("dragging");
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        Classes.Remove("dragging");
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        Classes.Remove("dragging");
        if (DataContext is not SignViewModel vm) return;
        if (!e.DataTransfer.Contains(DataFormat.File)) return;

        var files = e.DataTransfer.TryGetFiles();
        if (files is null) return;

        foreach (var item in files)
        {
            if (item is not IStorageFile file) continue;
            var path = file.TryGetLocalPath();
            if (path is null) continue;
            if (path.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase))
            {
                vm.SelectedFile = path;
                break;
            }
        }
    }
}
