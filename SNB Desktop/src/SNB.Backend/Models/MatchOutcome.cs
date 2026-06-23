namespace SNB.Backend.Models;

public sealed record MatchOutcome
{
    public DeviceMetadataEntry? Entry { get; init; }

    public MatchStrategy Strategy { get; init; }

    /// <summary>
    /// The device-side value that produced the match (original, human-readable form).
    /// </summary>
    public string MatchedValue { get; init; } = string.Empty;

    /// <summary>
    /// The Levenshtein ratio, populated only when <see cref="Strategy"/> is <see cref="MatchStrategy.Fuzzy"/>.
    /// </summary>
    public double? FuzzyScore { get; init; }

    public static MatchOutcome None { get; } = new() { Strategy = MatchStrategy.None };
}
