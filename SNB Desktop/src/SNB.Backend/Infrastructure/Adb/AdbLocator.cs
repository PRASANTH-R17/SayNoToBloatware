using System.Runtime.InteropServices;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Adb;

public sealed class AdbLocator
{
    private readonly IPathProvider _pathProvider;

    public AdbLocator(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public string GetAdbPath()
    {
        var baseDir = _pathProvider.BaseDirectory;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var adbPath = Path.Combine(baseDir, "adb.exe");
            if (!File.Exists(adbPath))
            {
                throw new FileNotFoundException($"Bundled adb.exe not found at {adbPath}");
            }

            return adbPath;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var adbPath = Path.Combine(baseDir, "adb");
            if (!File.Exists(adbPath))
            {
                throw new FileNotFoundException($"Bundled adb not found at {adbPath}");
            }

            return adbPath;
        }

        throw new PlatformNotSupportedException("Only Windows and Linux are supported.");
    }
}
