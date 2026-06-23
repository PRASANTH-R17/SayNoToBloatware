using System.Text.Json;
using SNB.Backend.Models.Bridge;

namespace SNB.Backend.Tests;

public class BridgeJsonParsingTests
{
    [Fact]
    public void DeserializeBridgeApp_MapsLabelToAppNameField()
    {
        const string json = """
            [
              {
                "packageName": "com.google.android.youtube",
                "label": "YouTube",
                "iconBase64": "aGVsbG8="
              }
            ]
            """;

        var apps = JsonSerializer.Deserialize<List<BridgeAppDto>>(json);
        Assert.NotNull(apps);
        Assert.Single(apps!);
        Assert.Equal("YouTube", apps![0].Label);
        Assert.Equal("com.google.android.youtube", apps[0].PackageName);
    }
}
