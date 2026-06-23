namespace SNB.Backend.Models;

public sealed class BloatwareInfo
{
    public required string PackageName { get; init; }
    public required string Description { get; init; }
    public required RemovalType RemovalType { get; init; }
    public required string Source { get; init; }
}
