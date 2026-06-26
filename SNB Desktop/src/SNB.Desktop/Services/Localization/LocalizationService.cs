using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using Avalonia.Platform;

namespace SNB.Desktop.Services.Localization;

/// <summary>Languages the UI can be displayed in.</summary>
public enum AppLanguage
{
    English,

    /// <summary>Code-mixed "Tanglish" (Tamil script + English words), shown as "தமிழ் (Easy)".</summary>
    Tamil,
}

/// <summary>
/// Lightweight runtime localization. Strings are keyed by stable dotted keys (e.g.
/// <c>Sidebar.SelectDevice</c>) and looked up through the <see cref="this[string]"/> indexer, which
/// XAML binds to via the <c>{l:Loc}</c> markup extension. Switching language raises an
/// <c>Item[]</c> change so every bound string refreshes live. Only the keys present in the bundled
/// tables are translated; everything else falls back to English.
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    /// <summary>Process-wide singleton consumed by the markup extension and view models.</summary>
    public static LocalizationService Instance { get; } = new();

    private readonly Dictionary<string, string> _english;
    private readonly Dictionary<string, string> _tamil;

    /// <summary>Maps an English source string back to its key, for translating runtime status messages.</summary>
    private readonly Dictionary<string, string> _englishToKey;

    private AppLanguage _currentLanguage = AppLanguage.English;

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationService()
    {
        _english = Load("avares://SNB.Desktop/Assets/i18n/en.json");
        _tamil = Load("avares://SNB.Desktop/Assets/i18n/ta.json");

        _englishToKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _english)
        {
            _englishToKey[pair.Value] = pair.Key;
        }
    }

    public AppLanguage CurrentLanguage
    {
        get => _currentLanguage;
        private set
        {
            if (_currentLanguage == value)
            {
                return;
            }

            _currentLanguage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        }
    }

    /// <summary>Resolves a key to the current language, falling back to English then to the key itself.</summary>
    public string this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (_currentLanguage == AppLanguage.Tamil
                && _tamil.TryGetValue(key, out var tamil)
                && !string.IsNullOrEmpty(tamil))
            {
                return tamil;
            }

            return _english.TryGetValue(key, out var english) ? english : key;
        }
    }

    public void SetLanguage(AppLanguage language) => CurrentLanguage = language;

    /// <summary>
    /// Translates a runtime-generated English string (e.g. an <see cref="IProgress{T}"/> status
    /// message) by reverse-resolving it to a key. Unknown strings are returned unchanged.
    /// </summary>
    public string Dynamic(string? english)
    {
        if (!string.IsNullOrEmpty(english) && _englishToKey.TryGetValue(english, out var key))
        {
            return this[key];
        }

        return english ?? string.Empty;
    }

    private static Dictionary<string, string> Load(string uri)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                   ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
