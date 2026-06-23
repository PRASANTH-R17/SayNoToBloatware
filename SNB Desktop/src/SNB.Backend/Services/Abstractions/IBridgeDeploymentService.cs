namespace SNB.Backend.Services.Abstractions;

public interface IBridgeDeploymentService
{
    Task DeployAsync(string serial, CancellationToken cancellationToken = default);
    Task StopAsync(string serial, CancellationToken cancellationToken = default);
}
