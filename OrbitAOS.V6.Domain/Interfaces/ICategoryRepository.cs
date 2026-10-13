using OrbitAOS.V6.Domain.Entities;

namespace OrbitAOS.V6.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Category entity operations.
    /// </summary>
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetActiveAsync();
        Task<Category> AddAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
