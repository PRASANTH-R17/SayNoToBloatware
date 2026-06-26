using Microsoft.Data.Sqlite;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Tests;

public sealed class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializeAsync_WhenLocalDatabaseMissing_SeedsFromDefaultDatabase()
    {
        var baseDirectory = CreateTempDirectory();
        try
        {
            var defaultDatabasePath = Path.Combine(baseDirectory, "Default", "snb.db");
            await CreateSeedDatabaseAsync(defaultDatabasePath, ["com.seeded.app"]);

            var initializer = new DatabaseInitializer(new TestPathProvider(baseDirectory));
            await initializer.InitializeAsync();

            Assert.True(File.Exists(initializer.DatabasePath));
            Assert.Equal(1, await GetPackageCountAsync(initializer.DatabasePath));
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenLocalDatabaseHasNoPackages_ReplacesWithDefaultDatabase()
    {
        var baseDirectory = CreateTempDirectory();
        try
        {
            var defaultDatabasePath = Path.Combine(baseDirectory, "Default", "snb.db");
            await CreateSeedDatabaseAsync(defaultDatabasePath, ["com.seeded.app"]);

            var localDatabasePath = Path.Combine(baseDirectory, "Cache", "snb.db");
            await CreateSeedDatabaseAsync(localDatabasePath, []);
            Assert.Equal(0, await GetPackageCountAsync(localDatabasePath));

            var initializer = new DatabaseInitializer(new TestPathProvider(baseDirectory));
            await initializer.InitializeAsync();

            Assert.Equal(1, await GetPackageCountAsync(initializer.DatabasePath));
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "snb-db-init-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteDirectory(string path)
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static async Task CreateSeedDatabaseAsync(string databasePath, IReadOnlyCollection<string> packageNames)
    {
        var parent = Path.GetDirectoryName(databasePath)!;
        Directory.CreateDirectory(parent);

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = """
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
            """;
        await createCommand.ExecuteNonQueryAsync();

        foreach (var packageName in packageNames)
        {
            await using var packageCommand = connection.CreateCommand();
            packageCommand.CommandText = """
                INSERT OR REPLACE INTO Package (PackageName, Description, Removal, Source)
                VALUES ($packageName, $description, $removal, $source);
                """;
            packageCommand.Parameters.AddWithValue("$packageName", packageName);
            packageCommand.Parameters.AddWithValue("$description", "Seeded package");
            packageCommand.Parameters.AddWithValue("$removal", "delete");
            packageCommand.Parameters.AddWithValue("$source", SnbBackendOptions.OemSourceName);
            await packageCommand.ExecuteNonQueryAsync();
        }

        await using var sourceCommand = connection.CreateCommand();
        sourceCommand.CommandText = """
            INSERT OR REPLACE INTO Source (Name, ETag, LastSyncUtc)
            VALUES ($name, $etag, $lastSyncUtc);
            """;
        sourceCommand.Parameters.AddWithValue("$name", SnbBackendOptions.OemSourceName);
        sourceCommand.Parameters.AddWithValue("$etag", "seed-etag");
        sourceCommand.Parameters.AddWithValue("$lastSyncUtc", DateTime.UtcNow.ToString("O"));
        await sourceCommand.ExecuteNonQueryAsync();
    }

    private static async Task<int> GetPackageCountAsync(string databasePath)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Package;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private sealed class TestPathProvider(string baseDirectory) : IPathProvider
    {
        public string BaseDirectory { get; } = baseDirectory;
        public string DataDirectory { get; } = baseDirectory;
    }
}
