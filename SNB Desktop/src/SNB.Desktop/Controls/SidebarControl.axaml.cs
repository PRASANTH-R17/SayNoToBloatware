using Avalonia.Controls;

namespace SNB.Desktop.Controls;

/// <summary>
/// State-aware navigation sidebar. Inherits its DataContext (the <c>MainWindowViewModel</c>) from
/// the shell window and binds directly to the shell's nav commands and device state.
/// </summary>
public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();
    }
}
