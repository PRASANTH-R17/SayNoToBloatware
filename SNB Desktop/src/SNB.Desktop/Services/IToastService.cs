namespace SNB.Desktop.Services;

public enum ToastKind
{
    Success,
    Error,
}

/// <summary>
/// Brief, non-blocking feedback shown at the bottom of the main window (e.g. after clearing the icon cache).
/// </summary>
public interface IToastService
{
    void Show(string message, ToastKind kind = ToastKind.Success);
}
