using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.BL.DataServices;

/// <summary>
/// Data service interface for User CRUD and search operations
/// </summary>
public interface IUserDataService : IDataService<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, InitUserDTO, UserDetailsDTO, UserSearchDTO>
{
}

/// <summary>
/// Data service for User entities with full CRUD, search, and validation support
/// </summary>
public class UserDataService : DataServiceBase<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, InitUserDTO, UserDetailsDTO, UserSearchDTO>, IUserDataService
{
    public UserDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<User> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, InitUserDTO, UserDetailsDTO, UserSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
