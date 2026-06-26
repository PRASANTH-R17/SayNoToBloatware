using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SNB.Desktop.Services.Localization;

namespace SNB.Desktop.Services.Preferences;

/// <summary>User-facing preferences persisted across app restarts.</summary>
public sealed class AppPreferences
{
    /// <summary>"Light" or "Dark".</summary>
    public string Theme { get; set; } = "Light";

    public AppLanguage Language { get; set; } = AppLanguage.English;

    public bool AutoUpdateDatabase { get; set; } = true;
}

/// <summary>
/// Lightweight JSON-backed preferences store (singleton), mirroring the
/// <see cref="LocalizationService"/> pattern. Persists to
/// <c>%AppData%/SayNoToBloatware/preferences.json</c>. All IO is best-effort so a missing or
/// corrupt file simply falls back to defaults and never crashes the app.
/// </summary>
public sealed class PreferencesService
{
    /// <summary>Process-wide singleton consumed by the app bootstrap and view models.</summary>
    public static PreferencesService Instance { get; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _filePath;

    private PreferencesService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SayNoToBloatware");
        _filePath = Path.Combine(dir, "preferences.json");
    }

    /// <summary>The in-memory preferences (populated by <see cref="Load"/>; defaults until then).</summary>
    public AppPreferences Current { get; private set; } = new();

    /// <summary>Reads preferences from disk into <see cref="Current"/>; returns the loaded value.</summary>
    public AppPreferences Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<AppPreferences>(json, JsonOptions);
                if (loaded is not null)
                {
                    Current = loaded;
                }
            }
        }
        catch
        {
            Current = new AppPreferences();
        }

        return Current;
    }

    public void SetTheme(string theme)
    {
        Current.Theme = theme;
        Save();
    }

    public void SetLanguage(AppLanguage language)
    {
        Current.Language = language;
        Save();
    }

    public void SetAutoUpdateDatabase(bool enabled)
    {
        Current.AutoUpdateDatabase = enabled;
        Save();
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_filePath, JsonSerializer.Serialize(Current, JsonOptions));
        }
        catch
        {
            // Best effort; preferences are non-critical and must never block the app.
        }
    }
}
