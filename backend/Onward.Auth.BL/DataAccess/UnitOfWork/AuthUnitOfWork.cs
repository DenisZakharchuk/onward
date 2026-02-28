using Onward.Base.DataAccess;
using Microsoft.Extensions.Logging;
using Onward.Auth.BL.DbContexts;

namespace Onward.Auth.BL.DataAccess.UnitOfWork;

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
