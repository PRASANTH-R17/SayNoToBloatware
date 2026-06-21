namespace SNB.Backend.Services.Implementations;

public sealed class AppPathProvider : Abstractions.IPathProvider
{
    public string BaseDirectory => AppContext.BaseDirectory;
}
