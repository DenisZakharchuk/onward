using System.Data;
using System.Reflection;

namespace Onward.Base.Sql;

/// <summary>
/// ADO.NET implementation of <see cref="ISearchDataExecutor{TEntity}"/>.
/// Executes pre-built parameterized SQL via a plain <see cref="IDbConnection"/>
/// and materializes rows by matching column names to entity property names
/// (case-insensitive).
/// </summary>
/// <typeparam name="TEntity">Entity type — must have a parameterless constructor.</typeparam>
public sealed class AdoNetSqlDataExecutor<TEntity> : ISearchDataExecutor<TEntity>
    where TEntity : class, new()
{
    private readonly IDbConnection _connection;

    // Cache property map per TEntity to avoid repeated reflection lookups.
    private static readonly IReadOnlyDictionary<string, PropertyInfo> PropertyMap =
        BuildPropertyMap();

    public AdoNetSqlDataExecutor(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TEntity>> FetchAsync(SqlQuery query, CancellationToken cancellationToken)
    {
        var results = new List<TEntity>();

        using var cmd = CreateCommand(query);
        using var reader = cmd.ExecuteReader();

        // Build ordinal → PropertyInfo mapping once per query (schema may vary with projections).
        var fieldCount = reader.FieldCount;
        var mapped = new PropertyInfo?[fieldCount];
        for (int i = 0; i < fieldCount; i++)
        {
            PropertyMap.TryGetValue(reader.GetName(i), out mapped[i]);
        }

        while (reader.Read())
        {
            var entity = new TEntity();
            for (int i = 0; i < fieldCount; i++)
            {
                if (mapped[i] is null) continue;
                var raw = reader.GetValue(i);
                if (raw is DBNull) continue;
                mapped[i]!.SetValue(entity, raw);
            }
            results.Add(entity);
        }

        return Task.FromResult<IReadOnlyList<TEntity>>(results);
    }

    /// <inheritdoc/>
    public Task<int> CountAsync(SqlQuery query, CancellationToken cancellationToken)
    {
        using var cmd = CreateCommand(query);
        var scalar = cmd.ExecuteScalar();
        var count = scalar is null or DBNull ? 0 : Convert.ToInt32(scalar);
        return Task.FromResult(count);
    }

    private IDbCommand CreateCommand(SqlQuery query)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = query.Sql;

        for (int i = 0; i < query.Parameters.Count; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = $"p{i}";
            p.Value = query.Parameters[i] ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        return cmd;
    }

    private static IReadOnlyDictionary<string, PropertyInfo> BuildPropertyMap()
    {
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanWrite)
                dict[prop.Name] = prop;
        }
        return dict;
    }
}
