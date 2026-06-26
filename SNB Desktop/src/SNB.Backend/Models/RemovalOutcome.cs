namespace SNB.Backend.Models;

/// <summary>
/// Result of attempting to remove a single package. <see cref="Disabled"/> is used when an app
/// cannot be uninstalled (e.g. OEM-restricted) but was disabled for the current user instead.
/// </summary>
public enum RemovalOutcome
{
    Removed,
    Disabled,
    Failed,
}
