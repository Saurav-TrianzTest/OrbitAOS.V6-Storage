using OrbitAOS.V6.Domain.Entities;

namespace OrbitAOS.V6.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Book entity operations.
    /// Follows the Repository pattern for data access abstraction.
    /// </summary>
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<IEnumerable<Book>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Book>> SearchAsync(string searchTerm);
        Task<Book> AddAsync(Book book);
        Task<Book> UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetTotalCountAsync();
    }
}
