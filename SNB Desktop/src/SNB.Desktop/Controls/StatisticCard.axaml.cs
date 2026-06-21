using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SNB.Desktop.Controls;

/// <summary>
/// Icon + big number + label tile used on the Applications page
/// (Total Apps / Recommended Removal / Apps with Alternatives).
/// Bind <see cref="Title"/>, <see cref="Value"/>, <see cref="IconData"/>, and <see cref="Accent"/>.
/// </summary>
public partial class StatisticCard : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Title), "Title");

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Value), "0");

    /// <summary>
    /// Vector icon geometry shown in the accent badge (see <c>Themes/Icons.axaml</c>),
    /// rendered through a <see cref="Avalonia.Controls.PathIcon"/>.
    /// </summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<StatisticCard, Geometry?>(nameof(IconData));

    public static readonly StyledProperty<IBrush> AccentProperty =
        AvaloniaProperty.Register<StatisticCard, IBrush>(nameof(Accent), new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)));

    public static readonly StyledProperty<IBrush> AccentSoftProperty =
        AvaloniaProperty.Register<StatisticCard, IBrush>(nameof(AccentSoft), new SolidColorBrush(Color.FromRgb(0xEF, 0xF4, 0xFF)));

    public StatisticCard()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public IBrush Accent
    {
        get => GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>Soft background tint behind the icon (typically a 10% accent).</summary>
    public IBrush AccentSoft
    {
        get => GetValue(AccentSoftProperty);
        set => SetValue(AccentSoftProperty, value);
    }
}
