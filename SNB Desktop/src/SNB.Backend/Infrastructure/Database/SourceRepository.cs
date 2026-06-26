using Microsoft.Data.Sqlite;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Database;

public sealed class SourceRepository : ISourceRepository
{
    private readonly IDatabaseInitializer _databaseInitializer;

    public SourceRepository(IDatabaseInitializer databaseInitializer)
    {
        _databaseInitializer = databaseInitializer;
    }

    public async Task<SourceSyncInfo?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, ETag, LastSyncUtc FROM Source WHERE Name = $name;";
        command.Parameters.AddWithValue("$name", name);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        DateTime? lastSync = null;
        if (!reader.IsDBNull(2) && DateTime.TryParse(reader.GetString(2), out var parsed))
        {
            lastSync = parsed;
        }

        return new SourceSyncInfo
        {
            Name = reader.GetString(0),
            ETag = reader.IsDBNull(1) ? null : reader.GetString(1),
            LastSyncUtc = lastSync
        };
    }

    public async Task UpsertAsync(string name, string? eTag, DateTime lastSyncUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Source (Name, ETag, LastSyncUtc)
            VALUES ($name, $etag, $lastSyncUtc)
            ON CONFLICT(Name) DO UPDATE SET
                ETag = excluded.ETag,
                LastSyncUtc = excluded.LastSyncUtc;
            """;
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$etag", (object?)eTag ?? DBNull.Value);
        command.Parameters.AddWithValue("$lastSyncUtc", lastSyncUtc.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection($"Data Source={_databaseInitializer.DatabasePath}");
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
