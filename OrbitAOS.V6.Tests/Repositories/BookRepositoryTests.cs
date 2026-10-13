using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Infrastructure.Data;
using OrbitAOS.V6.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;

namespace OrbitAOS.V6.Tests.Repositories
{
    /// <summary>
    /// Integration tests for BookRepository using EF Core InMemory database.
    /// Tests the Infrastructure layer data access in isolation.
    /// </summary>
    public class BookRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly BookRepository _repository;

        public BookRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new BookRepository(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var category = new Category { Id = 1, Name = "Technology", IsActive = true, CreatedAt = DateTime.UtcNow };
            _context.Categories.Add(category);

            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Quantity = 10, IsAvailable = true, CategoryId = 1, CreatedAt = DateTime.UtcNow },
                new Book { Id = 2, Title = "Design Patterns", Author = "Gang of Four", Price = 44.99m, Quantity = 5, IsAvailable = true, CategoryId = 1, CreatedAt = DateTime.UtcNow },
                new Book { Id = 3, Title = "The Pragmatic Programmer", Author = "David Thomas", Price = 39.99m, Quantity = 8, IsAvailable = false, CategoryId = 1, CreatedAt = DateTime.UtcNow }
            };
            _context.Books.AddRange(books);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBooks()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnBook()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Clean Code");
            result.Author.Should().Be("Robert C. Martin");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddBookAndReturnIt()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "New Book",
                Author = "New Author",
                Price = 19.99m,
                Quantity = 3,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(newBook);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("New Book");

            var allBooks = await _repository.GetAllAsync();
            allBooks.Should().HaveCount(4);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBook()
        {
            // Arrange
            var book = await _repository.GetByIdAsync(1);
            book!.Title = "Updated Clean Code";
            book.Price = 35.00m;

            // Act
            var result = await _repository.UpdateAsync(book);

            // Assert
            result.Title.Should().Be("Updated Clean Code");
            result.Price.Should().Be(35.00m);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBook()
        {
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            result.Should().BeNull();

            var allBooks = await _repository.GetAllAsync();
            allBooks.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnMatchingBooks()
        {
            // Act
            var result = await _repository.SearchAsync("Clean");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnBooksInCategory()
        {
            // Act
            var result = await _repository.GetByCategoryAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetTotalCountAsync_ShouldReturnCorrectCount()
        {
            // Act
            var result = await _repository.GetTotalCountAsync();

            // Assert
            result.Should().Be(3);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
