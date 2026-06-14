using Microsoft.Data.Sqlite;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Database;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IPathProvider _pathProvider;

    public DatabaseInitializer(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public string DatabasePath =>
        Path.Combine(_pathProvider.BaseDirectory, "Cache", "snb.db");

    private string DefaultDatabasePath =>
        Path.Combine(_pathProvider.BaseDirectory, "Default", "snb.db");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var cacheDir = Path.GetDirectoryName(DatabasePath)!;
        Directory.CreateDirectory(cacheDir);
        Directory.CreateDirectory(Path.Combine(cacheDir, "Icons"));

        await SeedDatabaseFromDefaultIfNeededAsync(cancellationToken);

        await using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Package (
                PackageName TEXT PRIMARY KEY,
                Description TEXT,
                Removal TEXT,
                Source TEXT
            );
            CREATE TABLE IF NOT EXISTS Source (
                Name TEXT PRIMARY KEY,
                ETag TEXT,
                LastSyncUtc TEXT
            );
            CREATE TABLE IF NOT EXISTS AppIcon (
                PackageName TEXT PRIMARY KEY,
                IconPath TEXT,
                CreatedUtc TEXT
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task SeedDatabaseFromDefaultIfNeededAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(DefaultDatabasePath))
        {
            return;
        }

        if (!File.Exists(DatabasePath))
        {
            File.Copy(DefaultDatabasePath, DatabasePath, overwrite: false);
            return;
        }

        if (await IsPackageTableEmptyAsync(cancellationToken))
        {
            File.Copy(DefaultDatabasePath, DatabasePath, overwrite: true);
        }
    }

    private async Task<bool> IsPackageTableEmptyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            await connection.OpenAsync(cancellationToken);

            await using var tableCheckCommand = connection.CreateCommand();
            tableCheckCommand.CommandText = """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table' AND name = 'Package';
                """;
            var tableCount = Convert.ToInt64(await tableCheckCommand.ExecuteScalarAsync(cancellationToken) ?? 0);
            if (tableCount == 0)
            {
                return true;
            }

            await using var packageCountCommand = connection.CreateCommand();
            packageCountCommand.CommandText = "SELECT COUNT(*) FROM Package;";
            var packageCount = Convert.ToInt64(await packageCountCommand.ExecuteScalarAsync(cancellationToken) ?? 0);
            return packageCount == 0;
        }
        catch (SqliteException)
        {
            return true;
        }
    }
}
