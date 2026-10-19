// ============================================================
// Repository.cs – Infrastructure Layer
// Generic EF Core repository implementation.
// Implements IRepository<T> defined in OrbitAOS.Domain.
//
// Clean Architecture: Infrastructure implements Domain contracts.
// ============================================================

using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Common;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;

namespace OrbitAOS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic EF Core repository providing standard CRUD operations.
    /// Extend with domain-specific repositories for complex queries.
    /// </summary>
    /// <typeparam name="T">Domain entity inheriting from <see cref="BaseEntity"/>.</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        /// <inheritdoc/>
        public void Update(T entity)
        {
            entity.Touch(); // update audit timestamp
            _dbSet.Update(entity);
        }

        /// <inheritdoc/>
        public void Delete(T entity)
            => _dbSet.Remove(entity);

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
