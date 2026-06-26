using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Implementations;

namespace SNB.Backend.Tests;

public class DeviceImageMatcherTests
{
    [Fact]
    public void Match_BrandMarketNameExact_ReturnsBrandMarketName()
    {
        var matcher = CreateMatcher();
        var device = CreateDevice(brand: "Samsung", model: "SM-G991B", marketName: "Galaxy S21");
        var entries = new List<DeviceMetadataEntry>
        {
            Entry("samsung-galaxy-s21", "Samsung Galaxy S21", "Samsung"),
            Entry("acer-iconia", "Acer Iconia", "Acer")
        };

        var outcome = matcher.Match(device, entries);

        Assert.Equal(MatchStrategy.BrandMarketName, outcome.Strategy);
        Assert.Equal("samsung-galaxy-s21", outcome.Entry?.Slug);
        Assert.Equal("Samsung Galaxy S21", outcome.MatchedValue);
        Assert.Null(outcome.FuzzyScore);
    }

    [Fact]
    public void Match_BrandModelExact_ReturnsBrandModel()
    {
        var matcher = CreateMatcher();
        var device = CreateDevice(brand: "Acer", model: "A100", marketName: string.Empty);
        var entries = new List<DeviceMetadataEntry>
        {
            Entry("acer-a100", "Acer A100", "Acer")
        };

        var outcome = matcher.Match(device, entries);

        Assert.Equal(MatchStrategy.BrandModel, outcome.Strategy);
        Assert.Equal("acer-a100", outcome.Entry?.Slug);
        Assert.Equal("Acer A100", outcome.MatchedValue);
        Assert.Null(outcome.FuzzyScore);
    }

    [Fact]
    public void Match_ModelContainedInName_ReturnsPartial()
    {
        var matcher = CreateMatcher();
        var device = CreateDevice(brand: "Google", model: "Pixel", marketName: string.Empty);
        var entries = new List<DeviceMetadataEntry>
        {
            Entry("google-pixel-7-pro", "Google Pixel 7 Pro", "Google")
        };

        var outcome = matcher.Match(device, entries);

        Assert.Equal(MatchStrategy.Partial, outcome.Strategy);
        Assert.Equal("google-pixel-7-pro", outcome.Entry?.Slug);
        Assert.Equal("Pixel", outcome.MatchedValue);
        Assert.Null(outcome.FuzzyScore);
    }

    [Fact]
    public void Match_CloseName_ReturnsFuzzy()
    {
        var matcher = CreateMatcher();
        var device = CreateDevice(brand: "Samsung", model: "SM-G991B", marketName: "Galaxy S21");
        var entries = new List<DeviceMetadataEntry>
        {
            Entry("samsung-galaxy-s22", "Samsung Galaxy S22", "Samsung")
        };

        var outcome = matcher.Match(device, entries);

        Assert.Equal(MatchStrategy.Fuzzy, outcome.Strategy);
        Assert.Equal("samsung-galaxy-s22", outcome.Entry?.Slug);
        Assert.Equal("Samsung Galaxy S21", outcome.MatchedValue);
        Assert.NotNull(outcome.FuzzyScore);
    }

    [Fact]
    public void Match_NoRelation_ReturnsNone()
    {
        var matcher = CreateMatcher();
        var device = CreateDevice(brand: "Nokia", model: "XYZ999", marketName: "Nokia Random");
        var entries = new List<DeviceMetadataEntry>
        {
            Entry("apple-iphone-15", "Apple iPhone 15", "Apple")
        };

        var outcome = matcher.Match(device, entries);

        Assert.Equal(MatchStrategy.None, outcome.Strategy);
        Assert.Null(outcome.Entry);
        Assert.Equal(string.Empty, outcome.MatchedValue);
        Assert.Null(outcome.FuzzyScore);
    }

    private static DeviceImageMatcher CreateMatcher()
    {
        return new DeviceImageMatcher(
            NullLogger<DeviceImageMatcher>.Instance,
            Options.Create(new DeviceImageOptions()));
    }

    private static DeviceInfo CreateDevice(string brand, string model, string marketName)
    {
        return new DeviceInfo
        {
            Serial = "serial",
            Manufacturer = brand,
            Model = model,
            AndroidVersion = "14",
            Brand = brand,
            MarketName = marketName
        };
    }

    private static DeviceMetadataEntry Entry(string slug, string name, string brand)
    {
        return new DeviceMetadataEntry
        {
            Slug = slug,
            Name = name,
            Brand = brand,
            LocalImage = $"{slug}.png"
        };
    }
}
