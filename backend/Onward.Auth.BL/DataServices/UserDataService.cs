using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.User;
using Onward.Base.Abstractions;
using Onward.Base.DataAccess;
using Onward.Base.Services;
using Microsoft.Extensions.Logging;

namespace Onward.Auth.BL.DataServices;

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
        Onward.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<User> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, InitUserDTO, UserDetailsDTO, UserSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
