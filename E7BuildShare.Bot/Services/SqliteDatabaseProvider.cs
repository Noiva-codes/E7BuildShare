using Microsoft.Data.Sqlite;

namespace E7BuildShare.Bot.Services;

public sealed class SqliteDatabaseProvider
{
    private readonly DatabaseOptions _options;

    public SqliteDatabaseProvider(DatabaseOptions options) => _options = options;

    public SqliteConnection CreateConnection() =>
        new($"Data Source={Path.GetFullPath(_options.Path)}");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var databasePath = Path.GetFullPath(_options.Path);
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys = ON;
            CREATE TABLE IF NOT EXISTS SchemaMigrations (
                Version INTEGER NOT NULL PRIMARY KEY,
                AppliedAtUtc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Characters (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
                CreatedAtUtc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS BuildVersions (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                UploaderId TEXT NOT NULL,
                CharacterId INTEGER NOT NULL,
                StoragePath TEXT NOT NULL,
                OriginalFileName TEXT NOT NULL,
                UploadedAtUtc TEXT NOT NULL,
                FOREIGN KEY (CharacterId) REFERENCES Characters (Id)
            );
            CREATE INDEX IF NOT EXISTS IX_BuildVersions_UploaderCharacterDate
                ON BuildVersions (UploaderId, CharacterId, UploadedAtUtc DESC);
            CREATE INDEX IF NOT EXISTS IX_BuildVersions_CharacterDate
                ON BuildVersions (CharacterId, UploadedAtUtc DESC);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class DatabaseOptions
{
    public string Path { get; set; } = string.Empty;
}
