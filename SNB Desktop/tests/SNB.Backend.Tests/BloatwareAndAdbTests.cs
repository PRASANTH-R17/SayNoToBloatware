using System.Collections.Concurrent;
using SNB.Backend.Infrastructure.Adb;
using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;
using SNB.Backend.Services.Implementations;

namespace SNB.Backend.Tests;

public class AdbOutputParserTests
{
    [Fact]
    public void ParseDevices_SkipsHeaderAndParsesRows()
    {
        const string output = """
            List of devices attached
            RF8M123ABC	device
            emulator-5554	offline
            """;

        var devices = AdbOutputParser.ParseDevices(output);

        Assert.Equal(2, devices.Count);
        Assert.Equal(("RF8M123ABC", "device"), devices[0]);
        Assert.Equal(("emulator-5554", "offline"), devices[1]);
    }

    [Fact]
    public void ParsePackageList_ExtractsPackageNames()
    {
        const string output = """
            package:com.android.settings
            package:com.google.android.youtube
            """;

        var packages = AdbOutputParser.ParsePackageList(output);

        Assert.Equal(["com.android.settings", "com.google.android.youtube"], packages);
    }

    [Fact]
    public void IsUninstallSuccess_DetectsSuccess()
    {
        Assert.True(AdbOutputParser.IsUninstallSuccess("Success"));
        Assert.False(AdbOutputParser.IsUninstallSuccess("Failure [not installed for user 0]"));
    }

    [Fact]
    public void ExtractUninstallFailure_ReturnsMessageWhenNotSuccessful()
    {
        var failure = AdbOutputParser.ExtractUninstallFailure("Failure [not installed for user 0]");
        Assert.Equal("Failure [not installed for user 0]", failure);
    }

    [Fact]
    public void IsPackageInstalled_MatchesExactPackageLine()
    {
        const string output = "package:com.prasanth.snb.bridge";
        Assert.True(AdbOutputParser.IsPackageInstalled(output, "com.prasanth.snb.bridge"));
        Assert.False(AdbOutputParser.IsPackageInstalled(output, "com.other.app"));
    }
}

public class BloatwareMatcherTests
{
    [Fact]
    public void Match_PartitionsAppsIntoExpectedLists()
    {
        var cache = new FakeBloatwareCacheService(new Dictionary<string, BloatwareInfo>
        {
            ["com.delete.app"] = new BloatwareInfo
            {
                PackageName = "com.delete.app",
                Description = "Delete me",
                RemovalType = RemovalType.Delete,
                Source = "oem.json"
            },
            ["com.replace.app"] = new BloatwareInfo
            {
                PackageName = "com.replace.app",
                Description = "Replace me",
                RemovalType = RemovalType.Replace,
                Source = "oem.json"
            },
            ["com.caution.app"] = new BloatwareInfo
            {
                PackageName = "com.caution.app",
                Description = "Caution",
                RemovalType = RemovalType.Caution,
                Source = "misc.json"
            }
        });

        var matcher = new BloatwareMatcher(cache);
        var result = matcher.Match(
        [
            "com.delete.app",
            "com.replace.app",
            "com.caution.app",
            "com.clean.app"
        ]);

        Assert.Equal(4, result.AllApps.Count);
        Assert.Single(result.RecommendedRemovalApps);
        Assert.Equal("com.delete.app", result.RecommendedRemovalApps[0].PackageName);
        Assert.Single(result.AppsWithAlternatives);
        Assert.Equal("com.replace.app", result.AppsWithAlternatives[0].PackageName);
        Assert.Equal(3, result.AllApps.Count(a => a.IsBloatware));
        Assert.DoesNotContain(result.RecommendedRemovalApps, a => a.PackageName == "com.caution.app");
    }

    private sealed class FakeBloatwareCacheService : IBloatwareCacheService
    {
        private readonly ConcurrentDictionary<string, BloatwareInfo> _cache;

        public FakeBloatwareCacheService(IReadOnlyDictionary<string, BloatwareInfo> entries)
        {
            _cache = new ConcurrentDictionary<string, BloatwareInfo>(entries);
        }

        public int Count => _cache.Count;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public bool TryGet(string packageName, out BloatwareInfo? info) => _cache.TryGetValue(packageName, out info);
        public IReadOnlyDictionary<string, BloatwareInfo> GetAll() => _cache;
    }
}

public class BloatwareJsonParsingTests
{
    [Fact]
    public void DeserializeSampleEntries_MapsFields()
    {
        const string json = """
            [
              {
                "id": "com.example.bloat",
                "description": "Example bloatware app",
                "removal": "delete"
              }
            ]
            """;

        var entries = System.Text.Json.JsonSerializer.Deserialize<List<BloatwareEntry>>(json);
        Assert.NotNull(entries);
        Assert.Single(entries!);
        Assert.Equal("com.example.bloat", entries![0].Id);
        Assert.Equal("Example bloatware app", entries[0].Description);
        Assert.Equal(RemovalType.Delete, RemovalTypeMapper.FromString(entries[0].Removal));
    }
}

public class IconCacheServiceTests
{
    [Fact]
    public async Task SaveIconAsync_WritesFileAndTracksPackage()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "snb-test-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        try
        {
            var pathProvider = new TestPathProvider(tempDir);
            var repository = new InMemoryAppIconRepository();
            var service = new IconCacheService(pathProvider, repository);
            await service.LoadAsync();

            var missing = service.GetMissingPackages(["com.test.app"]);
            Assert.Single(missing);

            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var savedPath = await service.SaveIconAsync("com.test.app", png);

            Assert.NotNull(savedPath);
            Assert.True(File.Exists(savedPath));
            Assert.Equal(1, service.Count);
            Assert.Empty(service.GetMissingPackages(["com.test.app"]));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task ClearAsync_RemovesFilesAndInMemoryState()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "snb-test-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        try
        {
            var pathProvider = new TestPathProvider(tempDir);
            var repository = new InMemoryAppIconRepository();
            var service = new IconCacheService(pathProvider, repository);
            await service.LoadAsync();

            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            await service.SaveIconAsync("com.test.app", png);
            await service.SaveIconAsync("com.other.app", png);

            Assert.Equal(2, service.Count);

            var removed = await service.ClearAsync();

            Assert.Equal(2, removed);
            Assert.Equal(0, service.Count);
            Assert.Empty(Directory.GetFiles(service.IconsDirectory, "*.png"));
            Assert.Empty(await repository.GetAllPackageNamesAsync());
            Assert.Single(service.GetMissingPackages(["com.test.app"]));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class TestPathProvider(string baseDirectory) : IPathProvider
    {
        public string BaseDirectory { get; } = baseDirectory;
        public string DataDirectory { get; } = baseDirectory;
    }

    private sealed class InMemoryAppIconRepository : IAppIconRepository
    {
        private readonly HashSet<string> _packages = new(StringComparer.Ordinal);

        public Task UpsertAsync(string packageName, string iconPath, DateTime createdUtc, CancellationToken cancellationToken = default)
        {
            _packages.Add(packageName);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetAllPackageNamesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(_packages.ToList());
        }
        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            _packages.Clear();
            return Task.CompletedTask;
        }
    }
}
