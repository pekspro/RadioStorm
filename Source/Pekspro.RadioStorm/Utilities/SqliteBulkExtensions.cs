using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pekspro.RadioStorm.Utilities;

/// <summary>
/// Fast bulk insert and upsert for SQLite using a single prepared command.
/// Participates in the current <see cref="DbContext"/> transaction if there is one,
/// otherwise a local transaction is used. The change tracker is not affected.
/// </summary>
public static class SqliteBulkExtensions
{
    public static Task BulkInsertAsync<T>(
        this DbContext context,
        IEnumerable<T> entities,
        SqliteBulkMap<T> map,
        CancellationToken cancellationToken = default) where T : class
        => ExecuteAsync(context, entities, map, map.InsertSql, cancellationToken);

    /// <summary>
    /// Inserts entities, or updates all non key columns if a row with the same primary key already exists.
    /// </summary>
    public static Task BulkInsertOrUpdateAsync<T>(
        this DbContext context,
        IEnumerable<T> entities,
        SqliteBulkMap<T> map,
        CancellationToken cancellationToken = default) where T : class
        => ExecuteAsync(context, entities, map, map.UpsertSql, cancellationToken);

    private static async Task ExecuteAsync<T>(
        DbContext context,
        IEnumerable<T> entities,
        SqliteBulkMap<T> map,
        string sql,
        CancellationToken cancellationToken) where T : class
    {
        if (entities.TryGetNonEnumeratedCount(out int count) && count == 0)
        {
            return;
        }

        var database = context.Database;

        await database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var connection = (SqliteConnection)database.GetDbConnection();
            var outerTransaction = (SqliteTransaction?)database.CurrentTransaction?.GetDbTransaction();
            using var localTransaction = outerTransaction is null ? connection.BeginTransaction() : null;

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = outerTransaction ?? localTransaction;

            int columnCount = map.Columns.Count;
            var parameters = new SqliteParameter[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                parameters[i] = command.Parameters.Add(new SqliteParameter { ParameterName = SqliteBulkMap<T>.GetParameterName(i) });
            }

            command.Prepare();

            var values = new object?[columnCount];

            foreach (var entity in entities)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Array.Clear(values);
                map.WriteValues(entity, values);

                for (int i = 0; i < columnCount; i++)
                {
                    parameters[i].Value = values[i] ?? DBNull.Value;
                }

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            localTransaction?.Commit();
        }
        finally
        {
            await database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }
}
