namespace SNB.Backend.DependencyInjection;

public sealed class SnbBackendOptions
{
    public const string OemSourceName = "oem.json";
    public const string MiscSourceName = "misc.json";
    public const string OemSourceUrl = "https://raw.githubusercontent.com/PRASANTH-R17/android-debloat-list/master/oem.json";
    public const string MiscSourceUrl = "https://raw.githubusercontent.com/PRASANTH-R17/android-debloat-list/master/misc.json";

    public string BridgePackageName { get; set; } = "com.prasanth.snb.bridge";
    public int BridgePort { get; set; } = 5000;

    /// <summary>
    /// Version code of the bundled bridge APK. Used to skip re-installing the APK when the device
    /// already has an up-to-date bridge. Bump this whenever the bundled APK's versionCode changes.
    /// </summary>
    public int BridgeVersionCode { get; set; } = 1;
    public int IconQueryBatchSize { get; set; } = 50;
    public TimeSpan BridgeHealthTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
