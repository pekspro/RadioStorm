namespace Pekspro.RadioStorm.Utilities;

/// <summary>
/// Explicit, reflection-free mapping between an entity and a SQLite table.
/// Used by <see cref="SqliteBulkExtensions"/> to keep bulk writes trimming- and AOT-compatible.
/// </summary>
public abstract class SqliteBulkMap<T> where T : class
{
    private string? _InsertSql;
    private string? _UpsertSql;

    /// <param name="tableName">Name of the table.</param>
    /// <param name="keyColumns">Primary key columns.</param>
    /// <param name="columns">
    /// Columns that are written, in the same order as values are produced by <see cref="WriteValues"/>.
    /// Database generated columns (like auto increment keys) must not be included.
    /// </param>
    protected SqliteBulkMap(string tableName, string[] keyColumns, string[] columns)
    {
        if (columns.Length == 0)
        {
            throw new ArgumentException("At least one column is required.", nameof(columns));
        }

        TableName = tableName;
        KeyColumns = keyColumns;
        Columns = columns;
    }

    public string TableName { get; }

    public IReadOnlyList<string> KeyColumns { get; }

    public IReadOnlyList<string> Columns { get; }

    /// <summary>
    /// Writes provider values (already converted to SQLite compatible types) for
    /// each column in <see cref="Columns"/> into <paramref name="values"/>.
    /// </summary>
    public abstract void WriteValues(T entity, Span<object?> values);

    internal string InsertSql => _InsertSql ??= CreateInsertSql();

    internal string UpsertSql => _UpsertSql ??= CreateUpsertSql();

    private string CreateInsertSql()
    {
        var sql = new StringBuilder();

        sql.Append("INSERT INTO ").Append(Quote(TableName)).Append(" (");
        sql.AppendJoin(", ", Columns.Select(Quote));
        sql.Append(") VALUES (");
        sql.AppendJoin(", ", Enumerable.Range(0, Columns.Count).Select(GetParameterName));
        sql.Append(')');

        return sql.ToString();
    }

    private string CreateUpsertSql()
    {
#if DEBUG
        if (KeyColumns.Count == 0 || KeyColumns.Any(k => !Columns.Contains(k)))
        {
            throw new InvalidOperationException($"Insert or update on table {TableName} requires all key columns to be included in the written columns.");
        }
#endif

        var sql = new StringBuilder(CreateInsertSql());

        sql.Append(" ON CONFLICT (");
        sql.AppendJoin(", ", KeyColumns.Select(Quote));
        sql.Append(") DO ");

        var updateColumns = Columns.Where(c => !KeyColumns.Contains(c)).ToList();

        if (updateColumns.Count == 0)
        {
            sql.Append("NOTHING");
        }
        else
        {
            sql.Append("UPDATE SET ");
            sql.AppendJoin(", ", updateColumns.Select(c => $"{Quote(c)} = excluded.{Quote(c)}"));
        }

        return sql.ToString();
    }

    internal static string GetParameterName(int index) => "$p" + index;

    private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
