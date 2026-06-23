using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SNB.Backend.Models;
using SNB.Desktop.Models;

namespace SNB.Desktop.Services;

/// <summary>
/// Converts backend domain models (<see cref="DeviceInfo"/>, <see cref="DeviceListItem"/>,
/// <see cref="InstalledApp"/>, <see cref="DeviceApps"/>) into the UI-facing models the
/// view models bind to. Centralises all field mapping and derivation (size formatting,
/// category, warning text) so the view models stay thin.
/// </summary>
public static class BackendMapper
{
    private const string DevicePlaceholder = "avares://SNB.Desktop/Assets/device-placeholder.png";

    public static DeviceCardModel ToCardModel(DeviceListItem item)
        => ToCardModel(item.Device, item.Image.ImagePath);

    public static DeviceCardModel ToCardModel(DeviceInfo device, string? imagePath = null)
    {
        var name = !string.IsNullOrWhiteSpace(device.MarketName) ? device.MarketName : device.Model;
        return new DeviceCardModel
        {
            ImagePath = string.IsNullOrWhiteSpace(imagePath) ? DevicePlaceholder : imagePath,
            Name = name,
            AndroidVersion = device.AndroidVersion,
            Serial = device.Serial,
            Manufacturer = device.Manufacturer,
            Model = device.Model,
            SdkVersion = device.SdkVersion,
            BatteryPercent = device.BatteryPercent,
            Status = "Connected",
            FirstConnected = DateTime.Now.ToString("MMM d, yyyy h:mm tt", CultureInfo.CurrentCulture),
        };
    }

    public static ApplicationItemModel ToItemModel(InstalledApp app)
    {
        var category = app.BloatwareRemovalType switch
        {
            RemovalType.Delete => AppCategory.RecommendedRemoval,
            RemovalType.Replace => AppCategory.AppsWithAlternatives,
            _ => AppCategory.RegularApp,
        };

        return new ApplicationItemModel
        {
            IconPath = app.IconPath ?? string.Empty,
            AppName = string.IsNullOrWhiteSpace(app.AppName) ? app.PackageName : app.AppName,
            PackageName = app.PackageName,
            Category = category,
            Description = app.BloatwareDescription ?? string.Empty,
            Source = app.IsBloatware ? "bloatware list" : (app.IsSystem ? "system" : "user"),
            Version = app.Version,
            Size = FormatSize(app.SizeBytes),
            SizeBytes = app.SizeBytes,
            IsSystemApp = app.IsSystem,
            FirstFound = DateTime.Now.ToString("MMM d, yyyy h:mm tt", CultureInfo.CurrentCulture),
            Permissions = app.Permissions.ToList(),
            Warning = BuildWarning(app),
        };
    }

    public static IReadOnlyList<ApplicationItemModel> ToItemModels(IEnumerable<InstalledApp> apps)
        => apps.Select(ToItemModel).ToList();

    public static AppStatistics ToStatistics(DeviceApps apps)
        => new(
            TotalApps: apps.AllApps.Count,
            RecommendedRemoval: apps.RecommendedRemovalApps.Count,
            AppsWithAlternatives: apps.AppsWithAlternatives.Count);

    private static string BuildWarning(InstalledApp app)
    {
        return app.BloatwareRemovalType switch
        {
            RemovalType.Caution =>
                string.IsNullOrWhiteSpace(app.BloatwareDescription)
                    ? "Remove with caution. This package may affect other features."
                    : $"Remove with caution. {app.BloatwareDescription}",
            RemovalType.Unsafe =>
                string.IsNullOrWhiteSpace(app.BloatwareDescription)
                    ? "Unsafe to remove. Removing this package may break core device functionality."
                    : $"Unsafe to remove. {app.BloatwareDescription}",
            _ when app.IsSystem =>
                "This is a system app. It will be removed for the current user (user 0) rather than fully uninstalled.",
            _ => string.Empty,
        };
    }

    private static string FormatSize(long bytes)
    {
        if (bytes <= 0)
        {
            return string.Empty;
        }

        double value = bytes;
        if (value >= 1024L * 1024 * 1024)
        {
            return $"{value / (1024.0 * 1024 * 1024):0.##} GB";
        }

        if (value >= 1024 * 1024)
        {
            return $"{value / (1024.0 * 1024):0.#} MB";
        }

        if (value >= 1024)
        {
            return $"{value / 1024.0:0.#} KB";
        }

        return $"{bytes} B";
    }
}
