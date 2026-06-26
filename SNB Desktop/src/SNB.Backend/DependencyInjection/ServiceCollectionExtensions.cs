using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SNB.Backend.Infrastructure.Adb;
using SNB.Backend.Infrastructure.Bridge;
using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Services.Abstractions;
using SNB.Backend.Services.Implementations;

namespace SNB.Backend.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSnbBackend(
        this IServiceCollection services,
        Action<SnbBackendOptions>? configure = null,
        Action<DeviceImageOptions>? configureDeviceImage = null)
    {
        var optionsBuilder = services.AddOptions<SnbBackendOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        var deviceImageOptionsBuilder = services.AddOptions<DeviceImageOptions>();
        if (configureDeviceImage is not null)
        {
            deviceImageOptionsBuilder.Configure(configureDeviceImage);
        }

        services.TryAddSingleton<IPathProvider, AppPathProvider>();
        services.TryAddSingleton<AdbLocator>();
        services.TryAddSingleton<IAdbExecutor, AdbProcessRunner>();

        services.TryAddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.TryAddSingleton<IPackageRepository, PackageRepository>();
        services.TryAddSingleton<ISourceRepository, SourceRepository>();
        services.TryAddSingleton<IAppIconRepository, AppIconRepository>();

        services.AddHttpClient<IBloatwareSyncService, BloatwareSyncService>();
        services.TryAddSingleton<IBloatwareCacheService, BloatwareCacheService>();
        services.TryAddSingleton<IBloatwareMatcher, BloatwareMatcher>();
        services.TryAddSingleton<IIconCacheService, IconCacheService>();

        services.AddHttpClient<IBridgeHttpClient, BridgeHttpClient>();
        services.TryAddSingleton<IBridgeDeploymentService, BridgeDeploymentService>();
        services.TryAddSingleton<IAppRemovalService, AppRemovalService>();
        services.TryAddSingleton<IDeviceSessionOrchestrator, DeviceSessionOrchestrator>();

        services.AddHttpClient<IDeviceMetadataService, DeviceMetadataService>();
        services.AddHttpClient<IDeviceImageService, DeviceImageService>();
        services.TryAddSingleton<IDeviceImageMatcher, DeviceImageMatcher>();

        return services;
    }
}
