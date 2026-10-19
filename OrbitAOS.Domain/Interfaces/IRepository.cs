// ============================================================
// IRepository.cs – Domain Layer
// Generic repository interface (contract only).
// Implementations live in OrbitAOS.Infrastructure.
//
// Clean Architecture rule: Domain defines the contract;
// Infrastructure provides the implementation.
// ============================================================

namespace OrbitAOS.Domain.Interfaces
{
    /// <summary>
    /// Generic repository contract for CRUD operations on domain entities.
    /// </summary>
    /// <typeparam name="T">A domain entity that inherits from <see cref="Common.BaseEntity"/>.</typeparam>
    public interface IRepository<T> where T : Common.BaseEntity
    {
        /// <summary>Retrieves an entity by its primary key.</summary>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Returns all entities of type T.</summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Adds a new entity to the repository.</summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing entity.</summary>
        void Update(T entity);

        /// <summary>Removes an entity from the repository.</summary>
        void Delete(T entity);

        /// <summary>Persists all pending changes to the underlying store.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
