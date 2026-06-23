namespace SNB.Backend.DependencyInjection;

public sealed class DeviceImageOptions
{
    public string MetadataUrl { get; set; } = "https://prasanth-r17.github.io/phone-image-dataset/devices.json";
    public string ImageBaseUrl { get; set; } = "https://pub-6002bd2cd38b48cc8bcf70440f17c83d.r2.dev/Images/";
    public string ImageFallbackBaseUrl { get; set; } = "https://prasanth-r17.github.io/phone-image-dataset/";

    public string MetadataCachePath { get; set; } = string.Empty;
    public string ImageCachePath { get; set; } = string.Empty;

    public double FuzzyThreshold { get; set; } = 0.6;

    public string DefaultImageRelativePath { get; set; } = "Assets/Images/default-phone.png";
    public string LoadingImageRelativePath { get; set; } = "Assets/Images/loading-phone.png";
}
