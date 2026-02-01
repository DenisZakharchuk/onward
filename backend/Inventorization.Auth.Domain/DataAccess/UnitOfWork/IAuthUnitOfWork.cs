using Inventorization.Auth.Domain.Entities;
using Inventorization.Base.DataAccess;
using IUserRepository = Inventorization.Auth.Domain.Services.Abstractions.IUserRepository;
using IRefreshTokenRepository = Inventorization.Auth.Domain.Services.Abstractions.IRefreshTokenRepository;

namespace Inventorization.Auth.Domain.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating repository operations
/// Repositories are registered directly in DI container
/// </summary>
public interface IAuthUnitOfWork : IUnitOfWork
{
}
