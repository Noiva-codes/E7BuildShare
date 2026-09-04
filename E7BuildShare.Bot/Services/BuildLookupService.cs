using Microsoft.Data.Sqlite;

namespace E7BuildShare.Bot.Services;

public sealed class BuildLookupService
{
    private readonly SqliteDatabaseProvider _database;

    public BuildLookupService(SqliteDatabaseProvider database) => _database = database;

    public async Task<IReadOnlyList<string>> GetCharacterNamesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM Characters ORDER BY Name COLLATE NOCASE;";
        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            names.Add(reader.GetString(0));
        return names;
    }

    public async Task<IReadOnlyList<ulong>> GetUploaderIdsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT UploaderId FROM BuildVersions ORDER BY UploaderId;";
        var ids = new List<ulong>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            if (ulong.TryParse(reader.GetString(0), out var id))
                ids.Add(id);
        return ids;
    }

    public async Task<BuildVersionRecord?> GetLatestBuildAsync(
        ulong uploaderId,
        string characterName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT b.Id, c.Name, b.StoragePath, b.OriginalFileName, b.UploadedAtUtc
            FROM BuildVersions b
            INNER JOIN Characters c ON c.Id = b.CharacterId
            WHERE b.UploaderId = $uploaderId AND c.Name = $characterName
            ORDER BY b.UploadedAtUtc DESC, b.Id DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$uploaderId", uploaderId.ToString());
        command.Parameters.AddWithValue("$characterName", characterName);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new BuildVersionRecord(
            reader.GetInt64(0), uploaderId, reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetString(4));
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = _database.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

public sealed record BuildVersionRecord(
    long Id,
    ulong UploaderId,
    string CharacterName,
    string StoragePath,
    string OriginalFileName,
    string UploadedAtUtc);
