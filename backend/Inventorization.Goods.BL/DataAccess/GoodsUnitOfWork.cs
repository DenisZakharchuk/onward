using Inventorization.Goods.BL.DbContexts;
using Inventorization.Base.DataAccess;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataAccess;

/// <summary>
/// Unit of Work interface for Goods bounded context
/// </summary>
public interface IGoodsUnitOfWork : Inventorization.Base.DataAccess.IUnitOfWork
{
}

/// <summary>
/// Unit of Work implementation for Goods bounded context.
/// Inherits transaction management and disposal logic from UnitOfWorkBase.
/// </summary>
public class GoodsUnitOfWork : UnitOfWorkBase<GoodsDbContext>, IGoodsUnitOfWork
{
    public GoodsUnitOfWork(GoodsDbContext context, ILogger<GoodsUnitOfWork> logger)
        : base(context, logger)
    {
    }
}
