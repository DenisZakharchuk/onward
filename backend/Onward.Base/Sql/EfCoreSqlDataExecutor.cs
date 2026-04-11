using Microsoft.EntityFrameworkCore;

namespace Onward.Base.Sql;

/// <summary>
/// EF Core implementation of <see cref="ISearchDataExecutor{TEntity}"/>.
/// Uses <c>DbSet&lt;TEntity&gt;.FromSqlRaw</c> to execute pre-built parameterized SQL
/// and materializes rows into entity instances tracked by the current <see cref="DbContext"/>.
/// </summary>
/// <remarks>
/// Only valid for entities that are registered in the <see cref="DbContext"/> model
/// (non-junction, non-keyless entities). The generator emits this executor only
/// for such entities.
/// </remarks>
/// <typeparam name="TEntity">Entity type — must be part of the <see cref="DbContext"/> model.</typeparam>
public sealed class EfCoreSqlDataExecutor<TEntity> : ISearchDataExecutor<TEntity>
    where TEntity : class
{
    private readonly DbContext _context;

    public EfCoreSqlDataExecutor(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TEntity>> FetchAsync(SqlQuery query, CancellationToken cancellationToken)
    {
        var results = await _context.Set<TEntity>()
            .FromSqlRaw(query.Sql, query.Parameters.Cast<object>().ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return results;
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(SqlQuery query, CancellationToken cancellationToken)
    {
        var result = await _context.Database
            .SqlQueryRaw<int>(query.Sql, query.Parameters.Cast<object>().ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}
