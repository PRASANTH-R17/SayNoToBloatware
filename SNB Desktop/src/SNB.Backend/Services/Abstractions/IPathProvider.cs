namespace SNB.Backend.Services.Abstractions;

public interface IPathProvider
{
    /// <summary>
    /// Read-only application directory (where the app and its bundled assets are installed).
    /// </summary>
    string BaseDirectory { get; }

    /// <summary>
    /// Writable per-user directory for runtime data (cache, working database, downloaded icons).
    /// Lives outside the install folder so it works when the app is installed to a read-only
    /// location such as Program Files.
    /// </summary>
    string DataDirectory { get; }
}
