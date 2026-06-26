using Microsoft.Data.Sqlite;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Database;

public sealed class PackageRepository : IPackageRepository
{
    private readonly IDatabaseInitializer _databaseInitializer;

    public PackageRepository(IDatabaseInitializer databaseInitializer)
    {
        _databaseInitializer = databaseInitializer;
    }

    public async Task UpsertAsync(BloatwareInfo package, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Package (PackageName, Description, Removal, Source)
            VALUES ($packageName, $description, $removal, $source)
            ON CONFLICT(PackageName) DO UPDATE SET
                Description = excluded.Description,
                Removal = excluded.Removal,
                Source = excluded.Source;
            """;
        BindPackage(command, package);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertIfNotFromSourceAsync(BloatwareInfo package, string protectedSource, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Package (PackageName, Description, Removal, Source)
            VALUES ($packageName, $description, $removal, $source)
            ON CONFLICT(PackageName) DO UPDATE SET
                Description = excluded.Description,
                Removal = excluded.Removal,
                Source = excluded.Source
            WHERE Package.Source != $protectedSource;
            """;
        BindPackage(command, package);
        command.Parameters.AddWithValue("$protectedSource", protectedSource);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BloatwareInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<BloatwareInfo>();
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT PackageName, Description, Removal, Source FROM Package;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BloatwareInfo
            {
                PackageName = reader.GetString(0),
                Description = reader.GetString(1),
                RemovalType = RemovalTypeMapper.FromString(reader.GetString(2)),
                Source = reader.GetString(3)
            });
        }

        return results;
    }

    public async Task DeleteBySourceAsync(string source, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Package WHERE Source = $source;";
        command.Parameters.AddWithValue("$source", source);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection($"Data Source={_databaseInitializer.DatabasePath}");
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static void BindPackage(SqliteCommand command, BloatwareInfo package)
    {
        command.Parameters.AddWithValue("$packageName", package.PackageName);
        command.Parameters.AddWithValue("$description", package.Description);
        command.Parameters.AddWithValue("$removal", RemovalTypeMapper.ToString(package.RemovalType));
        command.Parameters.AddWithValue("$source", package.Source);
    }
}
