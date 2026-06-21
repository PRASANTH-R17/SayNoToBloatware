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

    public string ReleaseDate => "May 9, 2025";

    public string Build => "1.0.0.20250509.1";

    public string Developer => "Prasanth R";

    public string Language => "English";

    public string License => "MIT License";

    public string Copyright => "© 2025 Say No to Bloatware. All rights reserved.";

    public string LicenseNotice =>
        "Say No to Bloatware is open-source software licensed under the MIT License.";

    public string WebsiteUrl => "https://snb.dev";

    public string GitHubUrl => "https://github.com/prasanthrangan/snb";

    public string IssuesUrl => "https://github.com/prasanthrangan/snb/issues";

    public string DocumentationUrl => "https://github.com/prasanthrangan/snb#readme";

    public string LicenseUrl => "https://github.com/prasanthrangan/snb/blob/main/LICENSE";

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
