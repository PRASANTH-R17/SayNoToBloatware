using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class DeviceImageMatcher : IDeviceImageMatcher
{
    private readonly ILogger<DeviceImageMatcher> _logger;
    private readonly DeviceImageOptions _options;

    public DeviceImageMatcher(
        ILogger<DeviceImageMatcher> logger,
        IOptions<DeviceImageOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public MatchOutcome Match(
        DeviceInfo device,
        IReadOnlyList<DeviceMetadataEntry> entries)
    {
        if (entries.Count == 0)
        {
            return MatchOutcome.None;
        }

        // Original (non-normalized) values, kept for human-readable reporting.
        var brandMarketNameOriginal = $"{device.Brand} {device.MarketName}".Trim();
        var brandModelOriginal = $"{device.Brand} {device.Model}".Trim();
        var modelOriginal = device.Model ?? string.Empty;

        var brandMarketName = Normalize(brandMarketNameOriginal);
        var brandModel = Normalize(brandModelOriginal);
        var model = Normalize(modelOriginal);
        var normalizedBrand = Normalize(device.Brand);

        // When the device exposes no real marketing name, MarketName falls back to the model code.
        // A bare model code (e.g. "v2561") cannot meaningfully match a marketing-name dataset, so the
        // substring/fuzzy strategies would only ever produce a misleading guess. Detect that here and
        // skip those strategies, letting the caller fall back to the brand/generic default image.
        var hasRealMarketName = Normalize(device.MarketName).Length > 0
            && Normalize(device.MarketName) != model;

        // Strategy 1: Brand + MarketName exact vs name.
        if (!string.IsNullOrEmpty(brandMarketName))
        {
            foreach (var entry in entries)
            {
                if (Normalize(entry.Name) == brandMarketName)
                {
                    _logger.LogInformation(
                        "Device matched using {Strategy} with value {MatchedValue} -> {Slug}",
                        MatchStrategy.BrandMarketName, brandMarketNameOriginal, entry.Slug);
                    return new MatchOutcome
                    {
                        Entry = entry,
                        Strategy = MatchStrategy.BrandMarketName,
                        MatchedValue = brandMarketNameOriginal
                    };
                }
            }
        }

        // Strategy 2: Brand + Model exact vs name.
        if (!string.IsNullOrEmpty(brandModel))
        {
            foreach (var entry in entries)
            {
                if (Normalize(entry.Name) == brandModel)
                {
                    _logger.LogInformation(
                        "Device matched using {Strategy} with value {MatchedValue} -> {Slug}",
                        MatchStrategy.BrandModel, brandModelOriginal, entry.Slug);
                    return new MatchOutcome
                    {
                        Entry = entry,
                        Strategy = MatchStrategy.BrandModel,
                        MatchedValue = brandModelOriginal
                    };
                }
            }
        }

        // Strategy 3: Model contained in name or slug.
        if (hasRealMarketName && !string.IsNullOrEmpty(model))
        {
            foreach (var entry in entries)
            {
                var name = Normalize(entry.Name);
                var slug = Normalize(entry.Slug);
                if ((name.Length > 0 && name.Contains(model, StringComparison.Ordinal))
                    || (slug.Length > 0 && slug.Contains(model, StringComparison.Ordinal)))
                {
                    _logger.LogInformation(
                        "Device matched using {Strategy} with value {MatchedValue} -> {Slug}",
                        MatchStrategy.Partial, modelOriginal, entry.Slug);
                    return new MatchOutcome
                    {
                        Entry = entry,
                        Strategy = MatchStrategy.Partial,
                        MatchedValue = modelOriginal
                    };
                }
            }
        }

        // Strategy 4: Fuzzy ratio vs name >= threshold (best match), scoped to the same brand.
        var target = string.IsNullOrEmpty(brandMarketName) ? brandModel : brandMarketName;
        var targetOriginal = string.IsNullOrEmpty(brandMarketName) ? brandModelOriginal : brandMarketNameOriginal;
        if (hasRealMarketName && !string.IsNullOrEmpty(target))
        {
            DeviceMetadataEntry? best = null;
            var bestRatio = 0.0;

            foreach (var entry in entries)
            {
                // Never fuzzy-match across brands - it is the main source of wrong images.
                if (normalizedBrand.Length > 0 && Normalize(entry.Brand) != normalizedBrand)
                {
                    continue;
                }

                var ratio = FuzzyRatio(target, Normalize(entry.Name));
                if (ratio > bestRatio)
                {
                    bestRatio = ratio;
                    best = entry;
                }
            }

            if (best is not null && bestRatio >= _options.FuzzyThreshold)
            {
                _logger.LogInformation(
                    "Device matched using {Strategy} with value {MatchedValue} -> {Slug} (score {Score:F3})",
                    MatchStrategy.Fuzzy, targetOriginal, best.Slug, bestRatio);
                return new MatchOutcome
                {
                    Entry = best,
                    Strategy = MatchStrategy.Fuzzy,
                    MatchedValue = targetOriginal,
                    FuzzyScore = bestRatio
                };
            }
        }

        // Strategy 5: None.
        return MatchOutcome.None;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                buffer[length++] = char.ToLowerInvariant(c);
            }
        }

        return new string(buffer[..length]);
    }

    private static double FuzzyRatio(string a, string b)
    {
        if (a.Length == 0 && b.Length == 0)
        {
            return 1.0;
        }

        if (a.Length == 0 || b.Length == 0)
        {
            return 0.0;
        }

        var distance = LevenshteinDistance(a, b);
        var maxLength = Math.Max(a.Length, b.Length);
        return 1.0 - ((double)distance / maxLength);
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
