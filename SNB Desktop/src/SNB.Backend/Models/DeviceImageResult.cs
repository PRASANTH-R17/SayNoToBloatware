namespace SNB.Backend.Models;

public sealed class DeviceImageResult
{
    public required string DeviceName { get; init; }
    public required string ImagePath { get; init; }
    public MetadataSource MetadataSource { get; init; }
    public ImageSource ImageSource { get; init; }
    public bool MatchFound { get; init; }
    public MatchStrategy MatchStrategy { get; init; }

    /// <summary>The device-side value that produced the match (empty when no match).</summary>
    public string MatchedValue { get; init; } = string.Empty;

    /// <summary>The slug of the matched catalog entry (empty when no match).</summary>
    public string MatchedEntrySlug { get; init; } = string.Empty;

    /// <summary>The Levenshtein ratio for fuzzy matches; null otherwise.</summary>
    public double? FuzzyScore { get; init; }
}
