using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using SNB.Desktop.Models;

namespace SNB.Desktop.Controls;

/// <summary>
/// Full device card used in the Device Selection list. Bind <see cref="Device"/> for the data and
/// <see cref="SelectCommand"/> for the "Select Device" button (the command receives the
/// <see cref="DeviceCardModel"/> as its parameter).
/// </summary>
public partial class DeviceCardControl : UserControl
{
    public static readonly StyledProperty<DeviceCardModel?> DeviceProperty =
        AvaloniaProperty.Register<DeviceCardControl, DeviceCardModel?>(nameof(Device));

    public static readonly StyledProperty<ICommand?> SelectCommandProperty =
        AvaloniaProperty.Register<DeviceCardControl, ICommand?>(nameof(SelectCommand));

    public DeviceCardControl()
    {
        InitializeComponent();
    }

    public DeviceCardModel? Device
    {
        get => GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    public ICommand? SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }
}
