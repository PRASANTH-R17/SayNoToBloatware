using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using SNB.Desktop.Services.Localization;

namespace SNB.Desktop.Markup;

/// <summary>
/// XAML markup extension that binds a target string property to a localization key, e.g.
/// <c>Text="{l:Loc Sidebar.SelectDevice}"</c>. Returns a one-way binding to
/// <see cref="LocalizationService"/>'s indexer so the value updates live when the language changes.
/// </summary>
public sealed class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key) => Key = key;

    /// <summary>The localization key to resolve (dotted, e.g. <c>Apps.TotalApps</c>).</summary>
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
        => new Binding
        {
            Path = $"[{Key}]",
            Source = LocalizationService.Instance,
            Mode = BindingMode.OneWay,
        };
}
