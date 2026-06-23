using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SNB.Desktop.Models;

namespace SNB.Desktop.Controls;

/// <summary>
/// Pill that color-codes an <see cref="AppCategory"/>:
///   RegularApp           -> green   ("Installed")
///   RecommendedRemoval   -> red     ("Recommended Removal")
///   AppsWithAlternatives -> amber   ("Has Alternatives")
/// Bind <see cref="Category"/>; the label/colors update automatically.
/// </summary>
public partial class CategoryBadge : UserControl
{
    public static readonly StyledProperty<AppCategory> CategoryProperty =
        AvaloniaProperty.Register<CategoryBadge, AppCategory>(nameof(Category));

    public static readonly StyledProperty<string> BadgeTextProperty =
        AvaloniaProperty.Register<CategoryBadge, string>(nameof(BadgeText), "INSTALLED");

    public static readonly StyledProperty<IBrush> BadgeBackgroundProperty =
        AvaloniaProperty.Register<CategoryBadge, IBrush>(nameof(BadgeBackground), MakeBrush(0xF0, 0xFD, 0xF4));

    public static readonly StyledProperty<IBrush> BadgeForegroundProperty =
        AvaloniaProperty.Register<CategoryBadge, IBrush>(nameof(BadgeForeground), MakeBrush(0x4A, 0xA8, 0x6E));

    public static readonly StyledProperty<bool> CompactProperty =
        AvaloniaProperty.Register<CategoryBadge, bool>(nameof(Compact), true);

    public static readonly StyledProperty<double> BadgeFontSizeProperty =
        AvaloniaProperty.Register<CategoryBadge, double>(nameof(BadgeFontSize), 10);

    public static readonly StyledProperty<Thickness> BadgePaddingProperty =
        AvaloniaProperty.Register<CategoryBadge, Thickness>(nameof(BadgePadding), new Thickness(10, 4));

    public static readonly StyledProperty<double> BadgeMinWidthProperty =
        AvaloniaProperty.Register<CategoryBadge, double>(nameof(BadgeMinWidth), 108);

    public CategoryBadge()
    {
        InitializeComponent();
        Apply(Category, Compact);
    }

    public AppCategory Category
    {
        get => GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public bool Compact
    {
        get => GetValue(CompactProperty);
        set => SetValue(CompactProperty, value);
    }

    public string BadgeText
    {
        get => GetValue(BadgeTextProperty);
        private set => SetValue(BadgeTextProperty, value);
    }

    public IBrush BadgeBackground
    {
        get => GetValue(BadgeBackgroundProperty);
        private set => SetValue(BadgeBackgroundProperty, value);
    }

    public IBrush BadgeForeground
    {
        get => GetValue(BadgeForegroundProperty);
        private set => SetValue(BadgeForegroundProperty, value);
    }

    public double BadgeFontSize
    {
        get => GetValue(BadgeFontSizeProperty);
        private set => SetValue(BadgeFontSizeProperty, value);
    }

    public Thickness BadgePadding
    {
        get => GetValue(BadgePaddingProperty);
        private set => SetValue(BadgePaddingProperty, value);
    }

    public double BadgeMinWidth
    {
        get => GetValue(BadgeMinWidthProperty);
        private set => SetValue(BadgeMinWidthProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CategoryProperty || change.Property == CompactProperty)
        {
            Apply(Category, Compact);
        }
    }

    private void Apply(AppCategory category, bool compact)
    {
        BadgeFontSize = compact ? 11 : 12;
        BadgePadding = compact ? new Thickness(10, 4) : new Thickness(12, 5);
        BadgeMinWidth = compact ? 132 : 0;

        switch (category)
        {
            case AppCategory.RecommendedRemoval:
                BadgeText = compact ? "Recommended" : "Recommended Removal";
                BadgeBackground = MakeBrush(0xFE, 0xE2, 0xE2);
                BadgeForeground = MakeBrush(0xDC, 0x26, 0x26);
                break;
            case AppCategory.AppsWithAlternatives:
                BadgeText = compact ? "Has Alternatives" : "Has Alternatives";
                BadgeBackground = MakeBrush(0xDB, 0xE1, 0xFF);
                BadgeForeground = MakeBrush(0x25, 0x63, 0xEB);
                break;
            default:
                BadgeText = compact ? "Installed" : "Installed";
                BadgeBackground = MakeBrush(0xF0, 0xFD, 0xF4);
                BadgeForeground = MakeBrush(0x16, 0xA3, 0x4A);
                break;
        }
    }

    private static SolidColorBrush MakeBrush(byte r, byte g, byte b) => new(Color.FromRgb(r, g, b));
}
