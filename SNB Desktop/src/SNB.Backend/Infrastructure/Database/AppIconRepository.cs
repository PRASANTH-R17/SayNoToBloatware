using Microsoft.Data.Sqlite;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Database;

public sealed class AppIconRepository : IAppIconRepository
{
    private readonly IDatabaseInitializer _databaseInitializer;

    public AppIconRepository(IDatabaseInitializer databaseInitializer)
    {
        _databaseInitializer = databaseInitializer;
    }

    public async Task UpsertAsync(string packageName, string iconPath, DateTime createdUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO AppIcon (PackageName, IconPath, CreatedUtc)
            VALUES ($packageName, $iconPath, $createdUtc)
            ON CONFLICT(PackageName) DO UPDATE SET
                IconPath = excluded.IconPath,
                CreatedUtc = excluded.CreatedUtc;
            """;
        command.Parameters.AddWithValue("$packageName", packageName);
        command.Parameters.AddWithValue("$iconPath", iconPath);
        command.Parameters.AddWithValue("$createdUtc", createdUtc.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllPackageNamesAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT PackageName FROM AppIcon;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM AppIcon;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection($"Data Source={_databaseInitializer.DatabasePath}");
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
