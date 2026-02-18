using Inventorization.Auth.BL.Entities;
using Inventorization.Base.DataAccess;
using IUserRepository = Inventorization.Auth.BL.Services.Abstractions.IUserRepository;
using IRefreshTokenRepository = Inventorization.Auth.BL.Services.Abstractions.IRefreshTokenRepository;

namespace Inventorization.Auth.BL.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating repository operations
/// Repositories are registered directly in DI container
/// </summary>
public interface IAuthUnitOfWork : IUnitOfWork
{
}
