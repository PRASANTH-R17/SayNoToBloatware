using System.Collections.Generic;
using SNB.Desktop.Models;

namespace SNB.Desktop.Services;

/// <summary>
/// Supplies mock devices and applications so the UI shell can be built and demoed
/// without a real ADB/backend connection. Later replaced by real backend workflows
/// (DeviceWorkflow / ApplicationWorkflow) behind the same view models.
/// </summary>
public interface IMockDataService
{
    /// <summary>The devices shown on the Device Selection page.</summary>
    IReadOnlyList<DeviceCardModel> GetDevices();

    /// <summary>The ~12-15 representative apps shown in the Applications grid.</summary>
    IReadOnlyList<ApplicationItemModel> GetApplications();

    /// <summary>Headline totals shown on the Applications statistic cards.</summary>
    AppStatistics GetStatistics();
}

/// <summary>Headline counts displayed on the Applications statistic cards.</summary>
public sealed record AppStatistics(int TotalApps, int RecommendedRemoval, int AppsWithAlternatives);
