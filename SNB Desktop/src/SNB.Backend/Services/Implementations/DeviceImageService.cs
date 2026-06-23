using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class DeviceImageService : IDeviceImageService
{
    private readonly HttpClient _httpClient;
    private readonly IDeviceMetadataService _metadataService;
    private readonly IDeviceImageMatcher _matcher;
    private readonly IPathProvider _pathProvider;
    private readonly ILogger<DeviceImageService> _logger;
    private readonly DeviceImageOptions _options;

    public DeviceImageService(
        HttpClient httpClient,
        IDeviceMetadataService metadataService,
        IDeviceImageMatcher matcher,
        IPathProvider pathProvider,
        ILogger<DeviceImageService> logger,
        IOptions<DeviceImageOptions> options)
    {
        _httpClient = httpClient;
        _metadataService = metadataService;
        _matcher = matcher;
        _pathProvider = pathProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<DeviceImageResult> ResolveAsync(DeviceInfo device, CancellationToken cancellationToken = default)
    {
        var defaultImagePath = ResolveDefaultImagePath(device);

        try
        {
            if (_metadataService.Source == MetadataSource.None)
            {
                await _metadataService.LoadAsync(cancellationToken);
            }

            if (_metadataService.Source == MetadataSource.None)
            {
                return BuildDefaultResult(device, defaultImagePath, _metadataService.Source);
            }

            var outcome = _matcher.Match(device, _metadataService.Entries);
            var entry = outcome.Entry;
            if (entry is null || outcome.Strategy == MatchStrategy.None)
            {
                _logger.LogInformation("Using default image");
                return BuildDefaultResult(device, defaultImagePath, _metadataService.Source);
            }

            var deviceName = string.IsNullOrWhiteSpace(entry.Name) ? device.DisplayName : entry.Name;

            var (imagePath, imageSource) = await ResolveImageAsync(entry, defaultImagePath, cancellationToken);

            return new DeviceImageResult
            {
                DeviceName = deviceName,
                ImagePath = imagePath,
                MetadataSource = _metadataService.Source,
                ImageSource = imageSource,
                MatchFound = true,
                MatchStrategy = outcome.Strategy,
                MatchedValue = outcome.MatchedValue,
                MatchedEntrySlug = entry.Slug,
                FuzzyScore = outcome.FuzzyScore
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Resolving device image failed; using default image.");
            _logger.LogInformation("Using default image");
            return BuildDefaultResult(device, defaultImagePath, _metadataService.Source);
        }
    }

    private async Task<(string ImagePath, ImageSource Source)> ResolveImageAsync(
        DeviceMetadataEntry entry,
        string defaultImagePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entry.LocalImage))
        {
            _logger.LogInformation("Using default image");
            return (defaultImagePath, ImageSource.Default);
        }

        var fileName = Path.GetFileName(entry.LocalImage);
        var cacheDirectory = ResolveImageCacheDirectory();
        var cachedImagePath = Path.Combine(cacheDirectory, fileName);

        if (File.Exists(cachedImagePath))
        {
            _logger.LogInformation("Image loaded from cache");
            return (cachedImagePath, ImageSource.Cache);
        }

        var primaryUrl = $"{_options.ImageBaseUrl.TrimEnd('/')}/{fileName}";
        if (await TryDownloadAsync(primaryUrl, cachedImagePath, cacheDirectory, cancellationToken))
        {
            _logger.LogInformation("Image downloaded from Cloudflare R2");
            return (cachedImagePath, ImageSource.Internet);
        }

        if (!string.IsNullOrWhiteSpace(_options.ImageFallbackBaseUrl))
        {
            var fallbackUrl = $"{_options.ImageFallbackBaseUrl.TrimEnd('/')}/{entry.LocalImage.TrimStart('/')}";
            if (await TryDownloadAsync(fallbackUrl, cachedImagePath, cacheDirectory, cancellationToken))
            {
                _logger.LogInformation("Image downloaded from GitHub Pages fallback");
                return (cachedImagePath, ImageSource.Internet);
            }
        }

        _logger.LogInformation("Using default image");
        return (defaultImagePath, ImageSource.Default);
    }

    private async Task<bool> TryDownloadAsync(
        string url, string cachedImagePath, string cacheDirectory, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url, cancellationToken);
            Directory.CreateDirectory(cacheDirectory);
            await File.WriteAllBytesAsync(cachedImagePath, bytes, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Image download failed from {Url}", url);
            return false;
        }
    }

    private static DeviceImageResult BuildDefaultResult(
        DeviceInfo device,
        string defaultImagePath,
        MetadataSource metadataSource)
    {
        return new DeviceImageResult
        {
            DeviceName = device.DisplayName,
            ImagePath = defaultImagePath,
            MetadataSource = metadataSource,
            ImageSource = ImageSource.Default,
            MatchFound = false,
            MatchStrategy = MatchStrategy.None
        };
    }

    private string ResolveDefaultImagePath(DeviceInfo device)
    {
        var baseDir = _pathProvider.BaseDirectory;
        var brand = new string((device.Brand ?? string.Empty).ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        if (brand.Length > 0)
        {
            var brandPath = Path.Combine(baseDir, "Assets", "Images", $"default-{brand}.png");
            if (File.Exists(brandPath)) return brandPath;
        }
        return Path.Combine(baseDir, _options.DefaultImageRelativePath);
    }

    private string ResolveImageCacheDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_options.ImageCachePath))
        {
            return _options.ImageCachePath;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "SNB", "Cache", "PhoneImages");
    }
}
