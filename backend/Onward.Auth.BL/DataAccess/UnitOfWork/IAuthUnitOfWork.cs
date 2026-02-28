using Onward.Auth.BL.Entities;
using Onward.Base.DataAccess;
using IUserRepository = Onward.Auth.BL.Services.Abstractions.IUserRepository;
using IRefreshTokenRepository = Onward.Auth.BL.Services.Abstractions.IRefreshTokenRepository;

namespace Onward.Auth.BL.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating repository operations
/// Repositories are registered directly in DI container
/// </summary>
public interface IAuthUnitOfWork : IUnitOfWork
{
}
