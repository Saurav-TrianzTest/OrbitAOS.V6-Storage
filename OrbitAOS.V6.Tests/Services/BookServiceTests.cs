using FluentAssertions;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Services;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Domain.Interfaces;
using Xunit;

namespace OrbitAOS.V6.Tests.Services
{
    /// <summary>
    /// Unit tests for BookService.
    /// Tests the application layer business logic in isolation using mocked repositories.
    /// </summary>
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _bookService = new BookService(_mockBookRepository.Object, _mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnAllBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Quantity = 10, IsAvailable = true },
                new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "David Thomas", Price = 34.99m, Quantity = 5, IsAvailable = true }
            };
            _mockBookRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            // Act
            var result = await _bookService.GetAllBooksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task GetBookByIdAsync_WithValidId_ShouldReturnBook()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Quantity = 10, IsAvailable = true };
            _mockBookRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _bookService.GetBookByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Clean Code");
            result.Author.Should().Be("Robert C. Martin");
        }

        [Fact]
        public async Task GetBookByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.GetBookByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateBookAsync_WithValidData_ShouldCreateAndReturnBook()
        {
            // Arrange
            var createDto = new CreateBookDto
            {
                Title = "New Book",
                Author = "New Author",
                Price = 19.99m,
                Quantity = 5,
                IsAvailable = true
            };

            var createdBook = new Book
            {
                Id = 1,
                Title = createDto.Title,
                Author = createDto.Author,
                Price = createDto.Price,
                Quantity = createDto.Quantity,
                IsAvailable = createDto.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            _mockBookRepository.Setup(r => r.AddAsync(It.IsAny<Book>())).ReturnsAsync(createdBook);

            // Act
            var result = await _bookService.CreateBookAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("New Book");
            result.Author.Should().Be("New Author");
            result.Price.Should().Be(19.99m);
            _mockBookRepository.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_WithValidData_ShouldUpdateAndReturnBook()
        {
            // Arrange
            var existingBook = new Book
            {
                Id = 1,
                Title = "Old Title",
                Author = "Old Author",
                Price = 10.00m,
                Quantity = 3,
                IsAvailable = true
            };

            var updateDto = new UpdateBookDto
            {
                Id = 1,
                Title = "Updated Title",
                Author = "Updated Author",
                Price = 25.00m,
                Quantity = 8,
                IsAvailable = true
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingBook);
            _mockBookRepository.Setup(r => r.UpdateAsync(It.IsAny<Book>())).ReturnsAsync(existingBook);

            // Act
            var result = await _bookService.UpdateBookAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            _mockBookRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

            var updateDto = new UpdateBookDto { Id = 999, Title = "Test", Author = "Test", Price = 10m };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.UpdateBookAsync(updateDto));
        }

        [Fact]
        public async Task DeleteBookAsync_WithValidId_ShouldDeleteBook()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockBookRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _bookService.DeleteBookAsync(1);

            // Assert
            _mockBookRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.DeleteBookAsync(999));
        }

        [Fact]
        public async Task SearchBooksAsync_WithValidTerm_ShouldReturnMatchingBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Quantity = 10, IsAvailable = true }
            };
            _mockBookRepository.Setup(r => r.SearchAsync("Clean")).ReturnsAsync(books);

            // Act
            var result = await _bookService.SearchBooksAsync("Clean");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task SearchBooksAsync_WithEmptyTerm_ShouldReturnAllBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1", Author = "Author 1", Price = 10m, Quantity = 1, IsAvailable = true },
                new Book { Id = 2, Title = "Book 2", Author = "Author 2", Price = 20m, Quantity = 2, IsAvailable = true }
            };
            _mockBookRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            // Act
            var result = await _bookService.SearchBooksAsync(string.Empty);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task BookExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _bookService.BookExistsAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetTotalBookCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(42);

            // Act
            var result = await _bookService.GetTotalBookCountAsync();

            // Assert
            result.Should().Be(42);
        }
    }
}
