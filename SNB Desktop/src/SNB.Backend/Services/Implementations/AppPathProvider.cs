namespace SNB.Backend.Services.Implementations;

public sealed class AppPathProvider : Abstractions.IPathProvider
{
    public string BaseDirectory => AppContext.BaseDirectory;

    public string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SayNoToBloatware");
}
