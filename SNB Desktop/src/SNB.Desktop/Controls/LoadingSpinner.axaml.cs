using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace SNB.Desktop.Controls;

/// <summary>Small circular arc spinner for inline loading states.</summary>
public partial class LoadingSpinner : UserControl
{
    public static readonly StyledProperty<IBrush> SpinnerForegroundProperty =
        AvaloniaProperty.Register<LoadingSpinner, IBrush>(
            nameof(SpinnerForeground),
            Brushes.Transparent);

    private readonly RotateTransform _rotateTransform = new();
    private readonly DispatcherTimer _timer;

    public LoadingSpinner()
    {
        InitializeComponent();

        RenderTransform = _rotateTransform;
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16),
        };
        _timer.Tick += OnTick;
    }

    public IBrush SpinnerForeground
    {
        get => GetValue(SpinnerForegroundProperty);
        set => SetValue(SpinnerForegroundProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _rotateTransform.Angle = (_rotateTransform.Angle + 8) % 360;
    }
}
