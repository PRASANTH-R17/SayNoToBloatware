using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace SNB.Desktop;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            // Global fallback so Tamil glyphs (used by the "தமிழ் (Easy)" language) render anywhere
            // the primary Latin font lacks coverage, without touching individual FontFamily values.
            // Points at the Fonts folder (not a single file) so all static weights of Noto Sans Tamil
            // (Regular/Medium/SemiBold/Bold) are discovered and the requested FontWeight is honored —
            // otherwise bold/semibold Tamil text would render at the default (light) weight.
            .With(new FontManagerOptions
            {
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily = new FontFamily(
                            "avares://SNB.Desktop/Assets/Fonts/#Noto Sans Tamil"),
                    },
                },
            })
            .LogToTrace();
}
