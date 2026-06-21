using Avalonia;
using Avalonia.Controls;

namespace SNB.Desktop.Controls;

/// <summary>
/// Reusable device image component with automatic fallback to default-phone.png.
/// Displays device-specific image if <see cref="ImagePath"/> is set, otherwise shows
/// the default placeholder from Assets/Images.
/// </summary>
public partial class DeviceImageControl : UserControl
{
    public static readonly StyledProperty<string?> ImagePathProperty =
        AvaloniaProperty.Register<DeviceImageControl, string?>(nameof(ImagePath));

    public DeviceImageControl()
    {
        InitializeComponent();
    }

    public string? ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }
}
