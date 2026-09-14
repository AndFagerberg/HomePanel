using System.Globalization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.AirPatrol;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.AirPatrol;

public sealed class SqliteAirPatrolHistoryRepository(
    IOptions<StorageOptions> options,
    TimeProvider timeProvider) : IAirPatrolHistoryRepository
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public async Task SaveAsync(AirPatrolStatus status, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var insert = connection.CreateCommand();
        insert.Transaction = (SqliteTransaction)transaction;
        insert.CommandText = """
            INSERT INTO AirPatrolHistory
                (RecordedAt, Name, Temperature, Humidity, Power, Mode, TargetTemperature, FanSpeed, Swing)
            VALUES
                ($recordedAt, $name, $temperature, $humidity, $power, $mode, $targetTemperature, $fanSpeed, $swing);
            """;
        insert.Parameters.AddWithValue("$recordedAt", status.UpdatedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        insert.Parameters.AddWithValue("$name", status.Name);
        insert.Parameters.AddWithValue("$temperature", status.Temperature);
        insert.Parameters.AddWithValue("$humidity", status.Humidity is { } humidity ? humidity : DBNull.Value);
        insert.Parameters.AddWithValue("$power", status.Power);
        insert.Parameters.AddWithValue("$mode", status.Mode);
        insert.Parameters.AddWithValue("$targetTemperature", status.TargetTemperature is { } target ? target : DBNull.Value);
        insert.Parameters.AddWithValue("$fanSpeed", status.FanSpeed);
        insert.Parameters.AddWithValue("$swing", status.Swing);
        await insert.ExecuteNonQueryAsync(cancellationToken);

        var delete = connection.CreateCommand();
        delete.Transaction = (SqliteTransaction)transaction;
        delete.CommandText = "DELETE FROM AirPatrolHistory WHERE RecordedAt < $cutoff;";
        delete.Parameters.AddWithValue(
            "$cutoff",
            timeProvider.GetUtcNow().Subtract(Retention).UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        await delete.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AirPatrolStatus>> GetSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Name, Temperature, Humidity, Power, Mode, TargetTemperature, FanSpeed, Swing, RecordedAt
            FROM AirPatrolHistory
            WHERE RecordedAt >= $since
            ORDER BY RecordedAt;
            """;
        command.Parameters.AddWithValue("$since", since.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

        var statuses = new List<AirPatrolStatus>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            statuses.Add(new AirPatrolStatus(
                reader.GetString(0),
                reader.GetDecimal(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.GetBoolean(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                reader.GetString(6),
                reader.GetBoolean(7),
                DateTimeOffset.Parse(reader.GetString(8), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)));
        }

        return statuses;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            var databasePath = GetDatabasePath();
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
            await using var connection = await OpenConnectionAsync(cancellationToken);
            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS AirPatrolHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RecordedAt TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Temperature REAL NOT NULL,
                    Humidity INTEGER NULL,
                    Power INTEGER NOT NULL,
                    Mode TEXT NOT NULL,
                    TargetTemperature REAL NULL,
                    FanSpeed TEXT NOT NULL,
                    Swing INTEGER NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_AirPatrolHistory_RecordedAt
                    ON AirPatrolHistory (RecordedAt);
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = GetDatabasePath(),
            Pooling = false,
        }.ToString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private string GetDatabasePath()
    {
        var configuredPath = options.Value.DatabasePath;
        return Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath) ? "data/homepanel.db" : configuredPath);
    }
}