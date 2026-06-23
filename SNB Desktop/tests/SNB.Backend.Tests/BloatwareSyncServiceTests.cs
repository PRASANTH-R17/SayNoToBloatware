using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Services.Abstractions;
using SNB.Backend.Services.Implementations;

namespace SNB.Backend.Tests;

public class BloatwareSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_On304_UpdatesLastSyncWithoutChangingPackages()
    {
        await using var fixture = await SqliteTestFixture.CreateAsync();
        await fixture.SeedSourceAsync(SnbBackendOptions.OemSourceName, "\"etag-v1\"", DateTime.UtcNow.AddDays(-1));
        await fixture.SeedPackageAsync("com.existing", "Existing", "delete", SnbBackendOptions.OemSourceName);

        var handler = new QueueHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotModified));

        var syncService = CreateSyncService(fixture, handler);
        await syncService.SyncAsync();

        var packages = await fixture.PackageRepository.GetAllAsync();
        Assert.Single(packages);
        var source = await fixture.SourceRepository.GetAsync(SnbBackendOptions.OemSourceName);
        Assert.NotNull(source?.LastSyncUtc);
    }

    [Fact]
    public async Task SyncAsync_On200_UpsertsPackagesAndStoresETag()
    {
        await using var fixture = await SqliteTestFixture.CreateAsync();

        const string json = """
            [
              {"id":"com.new.app","description":"New app","removal":"replace"}
            ]
            """;

        var handler = new QueueHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json),
            Headers = { ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"etag-v2\"") }
        });

        var syncService = CreateSyncService(fixture, handler);
        await syncService.SyncAsync();

        var oemPackages = (await fixture.PackageRepository.GetAllAsync())
            .Where(p => p.Source == SnbBackendOptions.OemSourceName)
            .ToList();

        Assert.Contains(oemPackages, p => p.PackageName == "com.new.app");
        var source = await fixture.SourceRepository.GetAsync(SnbBackendOptions.OemSourceName);
        Assert.Equal("etag-v2", source?.ETag);
    }

    [Fact]
    public async Task SyncAsync_MiscDoesNotOverwriteOemPackage()
    {
        await using var fixture = await SqliteTestFixture.CreateAsync();

        var handler = new QueueHandler(request =>
        {
            if (request.RequestUri!.AbsoluteUri.Contains("oem.json"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                        [{"id":"com.shared","description":"OEM desc","removal":"delete"}]
                        """),
                    Headers = { ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"oem\"") }
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"com.shared","description":"Misc desc","removal":"replace"}]
                    """),
                Headers = { ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"misc\"") }
            };
        });

        var syncService = CreateSyncService(fixture, handler);
        await syncService.SyncAsync();

        var package = Assert.Single(await fixture.PackageRepository.GetAllAsync());
        Assert.Equal("OEM desc", package.Description);
        Assert.Equal(SnbBackendOptions.OemSourceName, package.Source);
    }

    [Fact]
    public async Task SyncAsync_OnChangedSource_DeletesStaleRowsBeforeUpsert()
    {
        await using var fixture = await SqliteTestFixture.CreateAsync();
        await fixture.SeedSourceAsync(SnbBackendOptions.OemSourceName, "\"etag-old\"", DateTime.UtcNow.AddDays(-1));
        await fixture.SeedPackageAsync("com.old.app", "Old app", "delete", SnbBackendOptions.OemSourceName);

        var handler = new QueueHandler(request =>
        {
            if (request.RequestUri!.AbsoluteUri.Contains("oem.json"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                        [{"id":"com.new.app","description":"New app","removal":"replace"}]
                        """),
                    Headers = { ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"etag-new\"") }
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]"),
                Headers = { ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"misc-new\"") }
            };
        });

        var syncService = CreateSyncService(fixture, handler);
        await syncService.SyncAsync();

        var packages = await fixture.PackageRepository.GetAllAsync();
        Assert.DoesNotContain(packages, p => p.PackageName == "com.old.app");
        Assert.Contains(packages, p => p.PackageName == "com.new.app");
    }

    private static BloatwareSyncService CreateSyncService(SqliteTestFixture fixture, HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        return new BloatwareSyncService(
            httpClient,
            fixture.PackageRepository,
            fixture.SourceRepository,
            NullLogger<BloatwareSyncService>.Instance);
    }

    private sealed class QueueHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public QueueHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class SqliteTestFixture : IAsyncDisposable
    {
        private readonly string _directory;

        private SqliteTestFixture(string directory, IDatabaseInitializer databaseInitializer)
        {
            _directory = directory;
            DatabaseInitializer = databaseInitializer;
            PackageRepository = new PackageRepository(databaseInitializer);
            SourceRepository = new SourceRepository(databaseInitializer);
        }

        public IDatabaseInitializer DatabaseInitializer { get; }
        public PackageRepository PackageRepository { get; }
        public SourceRepository SourceRepository { get; }

        public static async Task<SqliteTestFixture> CreateAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), "snb-sync-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
            var initializer = new DatabaseInitializer(new TestPathProvider(directory));
            await initializer.InitializeAsync();
            return new SqliteTestFixture(directory, initializer);
        }

        public async Task SeedSourceAsync(string name, string eTag, DateTime lastSyncUtc)
        {
            await SourceRepository.UpsertAsync(name, eTag.Trim('"'), lastSyncUtc);
        }

        public async Task SeedPackageAsync(string packageName, string description, string removal, string source)
        {
            await PackageRepository.UpsertAsync(new Models.BloatwareInfo
            {
                PackageName = packageName,
                Description = description,
                RemovalType = RemovalTypeMapper.FromString(removal),
                Source = source
            });
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }
    }

    private sealed class TestPathProvider(string baseDirectory) : IPathProvider
    {
        public string BaseDirectory { get; } = baseDirectory;
    }
}
