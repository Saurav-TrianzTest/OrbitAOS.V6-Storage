using OrbitAOS.V6.Application.DTOs;

namespace OrbitAOS.V6.Application.Interfaces
{
    /// <summary>
    /// Service interface for Book business logic operations.
    /// Defines the application-level contract for book management.
    /// </summary>
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<IEnumerable<BookDto>> GetBooksByCategoryAsync(int categoryId);
        Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm);
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
        Task<BookDto> UpdateBookAsync(UpdateBookDto updateBookDto);
        Task DeleteBookAsync(int id);
        Task<bool> BookExistsAsync(int id);
        Task<int> GetTotalBookCountAsync();
    }
}
