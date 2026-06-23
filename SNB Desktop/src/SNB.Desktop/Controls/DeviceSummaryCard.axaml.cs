using Avalonia;
using Avalonia.Controls;
using SNB.Desktop.Models;

namespace SNB.Desktop.Controls;

/// <summary>
/// Compact device card shown in the sidebar after a device is selected:
/// image + name + Android pill + serial + Connected badge. Bind <see cref="Device"/>.
/// </summary>
public partial class DeviceSummaryCard : UserControl
{
    public static readonly StyledProperty<DeviceCardModel?> DeviceProperty =
        AvaloniaProperty.Register<DeviceSummaryCard, DeviceCardModel?>(nameof(Device));

    public DeviceSummaryCard()
    {
        InitializeComponent();
    }

    public DeviceCardModel? Device
    {
        get => GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }
}
