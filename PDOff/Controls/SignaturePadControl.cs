using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PDOff.Controls;

public class SignaturePadControl : Avalonia.Controls.Control
{
    private readonly List<List<Point>> _strokes = new();
    private List<Point>? _currentStroke;
    private bool _isExporting;

    public static readonly DirectProperty<SignaturePadControl, bool> HasSignatureProperty =
        AvaloniaProperty.RegisterDirect<SignaturePadControl, bool>(
            nameof(HasSignature), o => o.HasSignature);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<SignaturePadControl, double>(nameof(StrokeThickness), 2.5);

    private bool _hasSignature;

    public bool HasSignature
    {
        get => _hasSignature;
        private set => SetAndRaise(HasSignatureProperty, ref _hasSignature, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public SignaturePadControl()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        ClipToBounds = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _currentStroke = [e.GetPosition(this)];
            _strokes.Add(_currentStroke);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_currentStroke is not null)
        {
            _currentStroke.Add(e.GetPosition(this));
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_currentStroke is not null)
        {
            _currentStroke = null;
            HasSignature = _strokes.Count > 0;
            e.Handled = true;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_isExporting)
            context.DrawRectangle(Brushes.White, null, new Rect(Bounds.Size));

        var pen = new Pen(Brushes.Black, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
                context.DrawLine(pen, stroke[i - 1], stroke[i]);
        }
    }

    public void Clear()
    {
        _strokes.Clear();
        _currentStroke = null;
        HasSignature = false;
        InvalidateVisual();
    }

    public byte[]? ExportToPng()
    {
        if (_strokes.Count == 0) return null;

        var w = System.Math.Max(1, (int)Bounds.Width);
        var h = System.Math.Max(1, (int)Bounds.Height);

        _isExporting = true;
        using var bitmap = new RenderTargetBitmap(new PixelSize(w, h));
        bitmap.Render(this);
        _isExporting = false;

        using var ms = new MemoryStream();
        bitmap.Save(ms);
        return ms.ToArray();
    }
}
