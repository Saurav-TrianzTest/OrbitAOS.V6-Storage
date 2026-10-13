using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Domain.Interfaces;

namespace OrbitAOS.V6.Application.Services
{
    /// <summary>
    /// Implementation of IBookService containing book management business logic.
    /// Migrated from legacy ASP.NET MVC service pattern to ASP.NET Core DI-compatible service.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return books.Select(MapToDto);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book == null ? null : MapToDto(book);
        }

        public async Task<IEnumerable<BookDto>> GetBooksByCategoryAsync(int categoryId)
        {
            var books = await _bookRepository.GetByCategoryAsync(categoryId);
            return books.Select(MapToDto);
        }

        public async Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllBooksAsync();

            var books = await _bookRepository.SearchAsync(searchTerm);
            return books.Select(MapToDto);
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
        {
            var book = new Book
            {
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                ISBN = createBookDto.ISBN,
                Description = createBookDto.Description,
                Price = createBookDto.Price,
                Quantity = createBookDto.Quantity,
                CategoryId = createBookDto.CategoryId,
                IsAvailable = createBookDto.IsAvailable,
                PublishedDate = createBookDto.PublishedDate,
                CreatedAt = DateTime.UtcNow
            };

            var createdBook = await _bookRepository.AddAsync(book);
            return MapToDto(createdBook);
        }

        public async Task<BookDto> UpdateBookAsync(UpdateBookDto updateBookDto)
        {
            var existingBook = await _bookRepository.GetByIdAsync(updateBookDto.Id)
                ?? throw new KeyNotFoundException($"Book with ID {updateBookDto.Id} not found.");

            existingBook.Title = updateBookDto.Title;
            existingBook.Author = updateBookDto.Author;
            existingBook.ISBN = updateBookDto.ISBN;
            existingBook.Description = updateBookDto.Description;
            existingBook.Price = updateBookDto.Price;
            existingBook.Quantity = updateBookDto.Quantity;
            existingBook.CategoryId = updateBookDto.CategoryId;
            existingBook.IsAvailable = updateBookDto.IsAvailable;
            existingBook.PublishedDate = updateBookDto.PublishedDate;
            existingBook.UpdatedAt = DateTime.UtcNow;

            var updatedBook = await _bookRepository.UpdateAsync(existingBook);
            return MapToDto(updatedBook);
        }

        public async Task DeleteBookAsync(int id)
        {
            if (!await _bookRepository.ExistsAsync(id))
                throw new KeyNotFoundException($"Book with ID {id} not found.");

            await _bookRepository.DeleteAsync(id);
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalBookCountAsync()
        {
            return await _bookRepository.GetTotalCountAsync();
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Description = book.Description,
                Price = book.Price,
                Quantity = book.Quantity,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.Name,
                IsAvailable = book.IsAvailable,
                PublishedDate = book.PublishedDate,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }
    }
}
