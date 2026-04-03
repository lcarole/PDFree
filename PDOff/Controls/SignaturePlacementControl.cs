using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PDOff.Controls;

public class SignaturePlacementControl : Control
{
    private const double HandleSize = 10.0;
    private const double MinNormSize = 0.03;

    private enum DragMode { None, Body, TopLeft, TopRight, BottomLeft, BottomRight }

    private DragMode _dragMode = DragMode.None;
    private Point _dragStartPoint;
    private Rect _rectAtDragStart;
    private Rect _pageRect; // where the PDF page is drawn in control coordinates

    // --- Avalonia Properties ---

    public static readonly StyledProperty<Bitmap?> PageBitmapProperty =
        AvaloniaProperty.Register<SignaturePlacementControl, Bitmap?>(nameof(PageBitmap));

    public static readonly StyledProperty<Bitmap?> SignatureBitmapProperty =
        AvaloniaProperty.Register<SignaturePlacementControl, Bitmap?>(nameof(SignatureBitmap));

    public static readonly StyledProperty<Rect> NormalizedRectProperty =
        AvaloniaProperty.Register<SignaturePlacementControl, Rect>(
            nameof(NormalizedRect),
            defaultValue: new Rect(0.6, 0.85, 0.3, 0.1));

    public Bitmap? PageBitmap
    {
        get => GetValue(PageBitmapProperty);
        set => SetValue(PageBitmapProperty, value);
    }

    public Bitmap? SignatureBitmap
    {
        get => GetValue(SignatureBitmapProperty);
        set => SetValue(SignatureBitmapProperty, value);
    }

    public Rect NormalizedRect
    {
        get => GetValue(NormalizedRectProperty);
        set => SetValue(NormalizedRectProperty, value);
    }

    public SignaturePlacementControl()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        ClipToBounds = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageBitmapProperty ||
            change.Property == SignatureBitmapProperty ||
            change.Property == NormalizedRectProperty)
            InvalidateVisual();
    }

    // Compute the rect within the control where the PDF page is displayed (centered, aspect-ratio preserved)
    private Rect ComputePageRect()
    {
        var bitmap = PageBitmap;
        if (bitmap is null)
            return new Rect(8, 8, Math.Max(1, Bounds.Width - 16), Math.Max(1, Bounds.Height - 16));

        double bw = bitmap.Size.Width;
        double bh = bitmap.Size.Height;
        double cw = Bounds.Width;
        double ch = Bounds.Height;
        double scale = Math.Min(cw / bw, ch / bh);
        double pw = bw * scale;
        double ph = bh * scale;
        return new Rect((cw - pw) / 2, (ch - ph) / 2, pw, ph);
    }

    private Rect GetSignatureDisplayRect()
    {
        var nr = NormalizedRect;
        return new Rect(
            _pageRect.X + nr.X * _pageRect.Width,
            _pageRect.Y + nr.Y * _pageRect.Height,
            nr.Width * _pageRect.Width,
            nr.Height * _pageRect.Height);
    }

    public override void Render(DrawingContext context)
    {
        _pageRect = ComputePageRect();

        // Background
        context.DrawRectangle(
            new SolidColorBrush(Color.FromRgb(180, 180, 180)),
            null,
            new Rect(Bounds.Size));

        // Page
        if (PageBitmap is not null)
        {
            // Drop shadow
            context.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                null,
                _pageRect.Inflate(2));
            context.DrawImage(PageBitmap, _pageRect);
        }
        else
        {
            context.DrawRectangle(
                Brushes.White,
                new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200))),
                _pageRect);
        }

        // Signature overlay
        var sigRect = GetSignatureDisplayRect();
        var accentColor = Color.FromRgb(0, 103, 192);
        var accentBrush = new SolidColorBrush(accentColor);

        if (SignatureBitmap is not null)
        {
            context.DrawImage(SignatureBitmap, sigRect);
        }
        else
        {
            context.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(50, 0, 103, 192)),
                null,
                sigRect);
        }

        // Border
        context.DrawRectangle(
            null,
            new Pen(accentBrush, 1.5, dashStyle: new DashStyle(new double[] { 4, 3 }, 0)),
            sigRect);

        // Corner handles
        DrawHandle(context, new Point(sigRect.X, sigRect.Y), accentBrush);
        DrawHandle(context, new Point(sigRect.Right, sigRect.Y), accentBrush);
        DrawHandle(context, new Point(sigRect.X, sigRect.Bottom), accentBrush);
        DrawHandle(context, new Point(sigRect.Right, sigRect.Bottom), accentBrush);
    }

    private void DrawHandle(DrawingContext ctx, Point center, IBrush accent)
    {
        var r = new Rect(
            center.X - HandleSize / 2,
            center.Y - HandleSize / 2,
            HandleSize,
            HandleSize);
        ctx.DrawRectangle(Brushes.White, new Pen(accent, 1.5), r);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var pos = e.GetPosition(this);
        var sigRect = GetSignatureDisplayRect();
        var nr = NormalizedRect;

        _dragMode = GetHandleAt(pos, sigRect);

        if (_dragMode == DragMode.None)
        {
            if (sigRect.Contains(pos))
            {
                _dragMode = DragMode.Body;
            }
            else if (_pageRect.Contains(pos))
            {
                // Center signature at click point
                double nx = (pos.X - _pageRect.X) / _pageRect.Width - nr.Width / 2;
                double ny = (pos.Y - _pageRect.Y) / _pageRect.Height - nr.Height / 2;
                NormalizedRect = ClampRect(new Rect(nx, ny, nr.Width, nr.Height));
                _dragMode = DragMode.Body;
            }
        }

        _dragStartPoint = pos;
        _rectAtDragStart = NormalizedRect;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragMode == DragMode.None) return;

        var pos = e.GetPosition(this);
        double dx = (pos.X - _dragStartPoint.X) / _pageRect.Width;
        double dy = (pos.Y - _dragStartPoint.Y) / _pageRect.Height;
        var r = _rectAtDragStart;

        Rect newRect = _dragMode switch
        {
            DragMode.Body        => new Rect(r.X + dx, r.Y + dy, r.Width, r.Height),
            DragMode.TopLeft     => new Rect(r.X + dx, r.Y + dy, r.Width - dx, r.Height - dy),
            DragMode.TopRight    => new Rect(r.X,      r.Y + dy, r.Width + dx, r.Height - dy),
            DragMode.BottomLeft  => new Rect(r.X + dx, r.Y,      r.Width - dx, r.Height + dy),
            DragMode.BottomRight => new Rect(r.X,      r.Y,      r.Width + dx, r.Height + dy),
            _                    => r
        };

        NormalizedRect = ClampRect(newRect);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragMode = DragMode.None;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private DragMode GetHandleAt(Point pos, Rect sigRect)
    {
        if (IsNear(pos, new Point(sigRect.X, sigRect.Y), HandleSize))          return DragMode.TopLeft;
        if (IsNear(pos, new Point(sigRect.Right, sigRect.Y), HandleSize))      return DragMode.TopRight;
        if (IsNear(pos, new Point(sigRect.X, sigRect.Bottom), HandleSize))     return DragMode.BottomLeft;
        if (IsNear(pos, new Point(sigRect.Right, sigRect.Bottom), HandleSize)) return DragMode.BottomRight;
        return DragMode.None;
    }

    private static bool IsNear(Point a, Point b, double threshold)
        => Math.Abs(a.X - b.X) <= threshold && Math.Abs(a.Y - b.Y) <= threshold;

    private static Rect ClampRect(Rect r)
    {
        double w = Math.Max(r.Width, MinNormSize);
        double h = Math.Max(r.Height, MinNormSize);
        double x = Math.Clamp(r.X, 0, 1 - w);
        double y = Math.Clamp(r.Y, 0, 1 - h);
        return new Rect(x, y, w, h);
    }
}
