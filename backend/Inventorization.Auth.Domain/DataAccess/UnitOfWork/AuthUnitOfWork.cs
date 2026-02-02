using Inventorization.Base.DataAccess;
using Microsoft.Extensions.Logging;
using Inventorization.Auth.Domain.DbContexts;

namespace Inventorization.Auth.Domain.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work implementation for Auth bounded context.
/// Inherits transaction management and disposal logic from UnitOfWorkBase.
/// </summary>
public class AuthUnitOfWork : UnitOfWorkBase<AuthDbContext>, IAuthUnitOfWork
{
    public AuthUnitOfWork(AuthDbContext context, ILogger<AuthUnitOfWork> logger)
        : base(context, logger)
    {
    }
}
