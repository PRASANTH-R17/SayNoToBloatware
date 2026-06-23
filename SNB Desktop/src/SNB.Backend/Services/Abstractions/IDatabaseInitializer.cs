namespace SNB.Backend.Services.Abstractions;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    string DatabasePath { get; }
}
