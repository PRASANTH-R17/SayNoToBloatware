using System.Collections.Generic;
using SNB.Desktop.Models;

namespace SNB.Desktop.Services;

/// <inheritdoc cref="IMockDataService"/>
public sealed class MockDataService : IMockDataService
{
    private const string DevicePlaceholder = "avares://SNB.Desktop/Assets/device-placeholder.png";

    public IReadOnlyList<DeviceCardModel> GetDevices() => new List<DeviceCardModel>
    {
        new()
        {
            ImagePath = DevicePlaceholder,
            Name = "Samsung Galaxy S24",
            AndroidVersion = "14",
            Serial = "R58M12345A",
            Manufacturer = "Samsung",
            Model = "SM-S921B",
            SdkVersion = "34",
            BatteryPercent = 78,
            Status = "Connected",
            FirstConnected = "Today, 9:14 AM",
        },
        new()
        {
            ImagePath = DevicePlaceholder,
            Name = "OnePlus 12",
            AndroidVersion = "15",
            Serial = "ZY2245678B",
            Manufacturer = "OnePlus",
            Model = "CPH2581",
            SdkVersion = "35",
            BatteryPercent = 82,
            Status = "Connected",
            FirstConnected = "Today, 9:20 AM",
        },
    };

    public AppStatistics GetStatistics() => new(TotalApps: 487, RecommendedRemoval: 42, AppsWithAlternatives: 11);

    public IReadOnlyList<ApplicationItemModel> GetApplications() => new List<ApplicationItemModel>
    {
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/whatsapp.png",
            AppName = "WhatsApp",
            PackageName = "com.whatsapp",
            Category = AppCategory.RegularApp,
            Version = "2.24.5.78",
            Size = "182 MB",
            Source = "user",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Messaging app used to send texts, voice notes, and make calls over the internet.",
            Permissions = new List<string> { "Camera", "Microphone", "Contacts", "Storage" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/facebook.png",
            AppName = "Facebook App Manager",
            PackageName = "com.facebook.appmanager",
            Category = AppCategory.RecommendedRemoval,
            Version = "473.0.0.12",
            Size = "96 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Background service that silently updates Facebook-family apps. Safe to remove if you do not use Facebook apps.",
            Warning = "Removing this will stop background updates for Facebook, Messenger, and Instagram.",
            Permissions = new List<string> { "Background data", "Install packages", "Network access" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/miui-analytics.png",
            AppName = "MIUI Analytics",
            PackageName = "com.miui.analytics",
            Category = AppCategory.RecommendedRemoval,
            Version = "2024.03.15",
            Size = "24 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Collects usage and diagnostic analytics for MIUI. Commonly removed for privacy.",
            Warning = "This is a system app. Removing it stops telemetry collection but is generally safe.",
            Permissions = new List<string> { "Usage stats", "Network access", "Read phone state" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/google-photos.png",
            AppName = "Google Photos",
            PackageName = "com.google.android.apps.photos",
            Category = AppCategory.AppsWithAlternatives,
            Version = "6.78.0.42",
            Size = "212 MB",
            Source = "alternatives.json",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Photo and video gallery with cloud backup. Several open-source gallery apps offer similar features.",
            Warning = "Removing will disable automatic cloud backup of your photos.",
            Permissions = new List<string> { "Storage", "Camera", "Location" },
            Alternatives = new List<AlternativeApp>
            {
                new() { Name = "Simple Gallery", PackageName = "com.simplemobiletools.gallery.pro", Description = "Privacy-friendly offline gallery." },
                new() { Name = "Aves Gallery", PackageName = "deckers.thibault.aves", Description = "Open-source gallery & metadata viewer." },
            },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/youtube.png",
            AppName = "YouTube",
            PackageName = "com.google.android.youtube",
            Category = AppCategory.AppsWithAlternatives,
            Version = "19.16.39",
            Size = "168 MB",
            Source = "alternatives.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Video streaming app. Alternative front-ends provide an ad-free experience.",
            Warning = "This is a system app on many devices; removal may require disabling instead.",
            Permissions = new List<string> { "Network access", "Microphone", "Storage" },
            Alternatives = new List<AlternativeApp>
            {
                new() { Name = "NewPipe", PackageName = "org.schabi.newpipe", Description = "Lightweight, ad-free YouTube front-end." },
            },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/samsung-members.png",
            AppName = "Samsung Members",
            PackageName = "com.samsung.android.voc",
            Category = AppCategory.RegularApp,
            Version = "15.0.10.3",
            Size = "58 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Support and community app for Samsung devices, including diagnostics.",
            Permissions = new List<string> { "Network access", "Read phone state" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/fm-radio.png",
            AppName = "FM Radio",
            PackageName = "com.sec.android.app.fm",
            Category = AppCategory.RecommendedRemoval,
            Version = "12.1.00",
            Size = "14 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Built-in FM radio receiver app. Rarely used and safe to remove.",
            Warning = "Removing disables the FM radio feature on this device.",
            Permissions = new List<string> { "Microphone", "Storage" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/onedrive.png",
            AppName = "OneDrive",
            PackageName = "com.microsoft.skydrive",
            Category = AppCategory.AppsWithAlternatives,
            Version = "6.62.2",
            Size = "124 MB",
            Source = "alternatives.json",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Microsoft cloud storage and file sync. Other sync clients are available.",
            Warning = "Removing will stop file synchronisation with your Microsoft account.",
            Permissions = new List<string> { "Storage", "Network access", "Accounts" },
            Alternatives = new List<AlternativeApp>
            {
                new() { Name = "Nextcloud", PackageName = "com.nextcloud.client", Description = "Self-hosted file sync and storage." },
                new() { Name = "Syncthing", PackageName = "com.nutomic.syncthingandroid", Description = "Peer-to-peer file synchronisation." },
            },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/ar-zone.png",
            AppName = "Samsung AR Zone",
            PackageName = "com.samsung.android.arzone",
            Category = AppCategory.RecommendedRemoval,
            Version = "5.6.2.6",
            Size = "72 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Augmented-reality effects and AR Emoji studio. Commonly removed bloatware.",
            Warning = "Removing disables AR Emoji and AR Doodle camera modes.",
            Permissions = new List<string> { "Camera", "Storage", "Microphone" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/spotify.png",
            AppName = "Spotify",
            PackageName = "com.spotify.music",
            Category = AppCategory.RegularApp,
            Version = "8.9.40.476",
            Size = "98 MB",
            Source = "user",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Music and podcast streaming service.",
            Permissions = new List<string> { "Storage", "Network access", "Bluetooth" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/bixby.png",
            AppName = "Bixby Voice",
            PackageName = "com.samsung.android.bixby.agent",
            Category = AppCategory.RecommendedRemoval,
            Version = "3.3.55.6",
            Size = "143 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Samsung's voice assistant. Often removed if you use a different assistant.",
            Warning = "Removing disables the Bixby key and voice wake-up.",
            Permissions = new List<string> { "Microphone", "Contacts", "Network access" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/edge.png",
            AppName = "Microsoft Edge",
            PackageName = "com.microsoft.emmx",
            Category = AppCategory.AppsWithAlternatives,
            Version = "125.0.2535.51",
            Size = "156 MB",
            Source = "alternatives.json",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Chromium-based web browser by Microsoft.",
            Permissions = new List<string> { "Network access", "Storage", "Location" },
            Alternatives = new List<AlternativeApp>
            {
                new() { Name = "Firefox", PackageName = "org.mozilla.firefox", Description = "Privacy-focused open-source browser." },
            },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/netflix.png",
            AppName = "Netflix",
            PackageName = "com.netflix.mediaclient",
            Category = AppCategory.RegularApp,
            Version = "8.110.0",
            Size = "134 MB",
            Source = "user",
            IsSystemApp = false,
            FirstFound = "Today, 9:14 AM",
            Description = "Video streaming service for movies and TV shows.",
            Permissions = new List<string> { "Network access", "Storage" },
        },
        new()
        {
            IconPath = "avares://SNB.Desktop/Assets/AppIcons/galaxy-store.png",
            AppName = "Galaxy Store",
            PackageName = "com.sec.android.app.samsungapps",
            Category = AppCategory.RecommendedRemoval,
            Version = "4.5.21.7",
            Size = "88 MB",
            Source = "oem.json",
            IsSystemApp = true,
            FirstFound = "Today, 9:14 AM",
            Description = "Samsung's alternative app store. Removable if you only use Google Play.",
            Warning = "Removing prevents updates to Samsung-exclusive apps.",
            Permissions = new List<string> { "Install packages", "Network access", "Storage" },
        },
    };
}
