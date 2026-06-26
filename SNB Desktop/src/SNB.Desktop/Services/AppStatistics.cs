namespace SNB.Desktop.Services;

/// <summary>Headline counts displayed on the Applications statistic cards.</summary>
public sealed record AppStatistics(int TotalApps, int RecommendedRemoval, int AppsWithAlternatives);
