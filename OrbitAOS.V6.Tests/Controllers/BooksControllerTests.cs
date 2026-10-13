using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Web.Controllers;
using Xunit;

namespace OrbitAOS.V6.Tests.Controllers
{
    /// <summary>
    /// Unit tests for BooksController.
    /// Tests the Web layer controller actions in isolation using mocked services.
    /// </summary>
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ILogger<BooksController>> _mockLogger;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockLogger = new Mock<ILogger<BooksController>>();
            _controller = new BooksController(_mockBookService.Object, _mockCategoryService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithBooks()
        {
            // Arrange
            var books = new List<BookDto>
            {
                new BookDto { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m },
                new BookDto { Id = 2, Title = "Design Patterns", Author = "Gang of Four", Price = 44.99m }
            };
            var categories = new List<CategoryDto>
            {
                new CategoryDto { Id = 1, Name = "Technology", IsActive = true }
            };

            _mockBookService.Setup(s => s.GetAllBooksAsync()).ReturnsAsync(books);
            _mockCategoryService.Setup(s => s.GetActiveCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public async Task Details_WithValidId_ShouldReturnViewWithBook()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<BookDto>().Subject;
            model.Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task Details_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBookByIdAsync(999)).ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.Details(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_POST_WithValidModel_ShouldRedirectToIndex()
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
            var createdBook = new BookDto { Id = 1, Title = "New Book", Author = "New Author", Price = 19.99m };

            _mockBookService.Setup(s => s.CreateBookAsync(createDto)).ReturnsAsync(createdBook);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_POST_WithInvalidModel_ShouldReturnView()
        {
            // Arrange
            var createDto = new CreateBookDto(); // Empty - will fail validation
            _controller.ModelState.AddModelError("Title", "Title is required.");

            var categories = new List<CategoryDto>();
            _mockCategoryService.Setup(s => s.GetActiveCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Edit_GET_WithValidId_ShouldReturnViewWithBook()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Quantity = 5, IsAvailable = true };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(book);

            var categories = new List<CategoryDto>();
            _mockCategoryService.Setup(s => s.GetActiveCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<UpdateBookDto>().Subject;
            model.Id.Should().Be(1);
            model.Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task Edit_GET_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBookByIdAsync(999)).ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.Edit(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnViewWithBook()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<BookDto>().Subject;
            model.Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ShouldRedirectToIndex()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(book);
            _mockBookService.Setup(s => s.DeleteBookAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _mockBookService.Verify(s => s.DeleteBookAsync(1), Times.Once);
        }
    }
}
