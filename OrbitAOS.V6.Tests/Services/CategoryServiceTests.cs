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
    /// Unit tests for CategoryService.
    /// </summary>
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Fiction", IsActive = true },
                new Category { Id = 2, Name = "Non-Fiction", IsActive = true }
            };
            _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Fiction", IsActive = true };
            _mockCategoryRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Fiction");
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockCategoryRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCategoryAsync_WithValidData_ShouldCreateCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Science", Description = "Science books", IsActive = true };
            var createdCategory = new Category { Id = 1, Name = "Science", Description = "Science books", IsActive = true };

            _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(createdCategory);

            // Act
            var result = await _categoryService.CreateCategoryAsync(categoryDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Science");
            _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockCategoryRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.DeleteCategoryAsync(999));
        }

        [Fact]
        public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            var activeCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Fiction", IsActive = true },
                new Category { Id = 2, Name = "History", IsActive = true }
            };
            _mockCategoryRepository.Setup(r => r.GetActiveAsync()).ReturnsAsync(activeCategories);

            // Act
            var result = await _categoryService.GetActiveCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(c => c.IsActive).Should().BeTrue();
        }
    }
}
