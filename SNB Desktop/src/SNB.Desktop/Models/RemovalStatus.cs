namespace SNB.Desktop.Models;

/// <summary>
/// Per-application state during the removal flow (progress / complete dialogs).
/// </summary>
public enum RemovalStatus
{
    Pending,
    Removing,
    Removed,
    Disabled,
    Failed,
}
