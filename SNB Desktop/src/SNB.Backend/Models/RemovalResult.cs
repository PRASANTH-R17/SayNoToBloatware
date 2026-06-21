namespace SNB.Backend.Models;

public sealed class RemovalResult
{
    public required string PackageName { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
