using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// About page — app info, metadata, and external project links.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    public string AppName => "Say No to Bloatware";

    public string Tagline => "Clean your Android device. Remove bloatware. Take control.";

    public string Version => "1.0.0";

    public string Description =>
        "A free and open-source desktop application to help you safely identify and remove unwanted or bloat applications from your Android devices.";

    public string Developer => "Prasanth R";

    public string Language => "English";

    public string License => "MIT License";

    public string Copyright => "© 2025 Say No to Bloatware. All rights reserved.";

    public string LicenseNotice =>
        "Say No to Bloatware is open-source software licensed under the MIT License.";

    public string WebsiteUrl => "https://snb.prasanth.online";

    public string GitHubUrl => "https://github.com/PRASANTH-R17/SayNoToBloatware";

    public string IssuesUrl => "https://github.com/PRASANTH-R17/SayNoToBloatware/issues";

    public string DocumentationUrl => "https://prasanth-r17.github.io/SayNoToBloatware/";

    public string LicenseUrl => "https://github.com/PRASANTH-R17/SayNoToBloatware/blob/main/LICENSE";

    [RelayCommand]
    private void OpenLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch
        {
            // Browser launch can fail in restricted environments; ignore silently.
        }
    }
}
