using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Implementations;

namespace SNB.Backend.Tests;

public class DeviceMetadataServiceTests
{
    private const string SampleJson = """
        [
          {"slug":"samsung-galaxy-s21","name":"Samsung Galaxy S21","local_image":"samsung-galaxy-s21.png","brand":"Samsung"}
        ]
        """;

    [Fact]
    public async Task LoadAsync_OnInternetSuccess_CachesAndReportsInternet()
    {
        using var temp = new TempCacheDirectory();
        var cachePath = Path.Combine(temp.Path, "devices.json");

        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SampleJson)
        });

        var service = CreateService(handler, cachePath);
        await service.LoadAsync();

        Assert.Equal(MetadataSource.Internet, service.Source);
        Assert.Single(service.Entries);
        Assert.True(File.Exists(cachePath));
    }

    [Fact]
    public async Task LoadAsync_WhenOfflineWithCache_ReportsCache()
    {
        using var temp = new TempCacheDirectory();
        var cachePath = Path.Combine(temp.Path, "devices.json");
        await File.WriteAllTextAsync(cachePath, SampleJson);

        var handler = new StubHandler(_ => throw new HttpRequestException("offline"));

        var service = CreateService(handler, cachePath);
        await service.LoadAsync();

        Assert.Equal(MetadataSource.Cache, service.Source);
        Assert.Single(service.Entries);
    }

    [Fact]
    public async Task LoadAsync_WhenOfflineWithoutCache_ReportsNone()
    {
        using var temp = new TempCacheDirectory();
        var cachePath = Path.Combine(temp.Path, "devices.json");

        var handler = new StubHandler(_ => throw new HttpRequestException("offline"));

        var service = CreateService(handler, cachePath);
        await service.LoadAsync();

        Assert.Equal(MetadataSource.None, service.Source);
        Assert.Empty(service.Entries);
    }

    private static DeviceMetadataService CreateService(HttpMessageHandler handler, string cachePath)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new DeviceImageOptions { MetadataCachePath = cachePath });
        return new DeviceMetadataService(httpClient, NullLogger<DeviceMetadataService>.Instance, options);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class TempCacheDirectory : IDisposable
    {
        public TempCacheDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "snb-metadata-" + Guid.NewGuid());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
