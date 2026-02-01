using Inventorization.Base.DataAccess;

namespace InventorySystem.DataAccess.Abstractions;

/// <summary>
/// IRepository is now defined in Inventorization.Base.DataAccess.IRepository
/// This using alias allows existing code to work without changes
/// </summary>
public interface IRepository<T> : Inventorization.Base.DataAccess.IRepository<T>
    where T : class
{
}

