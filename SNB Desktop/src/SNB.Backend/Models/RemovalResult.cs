namespace SNB.Backend.Models;

public sealed class RemovalResult
{
    public required string PackageName { get; init; }
    public required RemovalOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }

    /// <summary>True when the app was neutralized (uninstalled or disabled), i.e. not a failure.</summary>
    public bool Success => Outcome != RemovalOutcome.Failed;
}
