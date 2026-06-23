namespace SNB.Backend.Models;

public sealed class InstalledApp
{
    public required string PackageName { get; init; }
    public string AppName { get; set; } = string.Empty;
    public bool IsBloatware { get; init; }
    public RemovalType? BloatwareRemovalType { get; init; }
    public string? BloatwareDescription { get; init; }
    public string? IconPath { get; set; }
}
