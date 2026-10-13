using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Domain.Interfaces;

namespace OrbitAOS.V6.Application.Services
{
    /// <summary>
    /// Implementation of ICategoryService containing category management business logic.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : MapToDto(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                IsActive = categoryDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _categoryRepository.AddAsync(category);
            return MapToDto(created);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto categoryDto)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryDto.Id)
                ?? throw new KeyNotFoundException($"Category with ID {categoryDto.Id} not found.");

            existing.Name = categoryDto.Name;
            existing.Description = categoryDto.Description;
            existing.IsActive = categoryDto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _categoryRepository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            if (!await _categoryRepository.ExistsAsync(id))
                throw new KeyNotFoundException($"Category with ID {id} not found.");

            await _categoryRepository.DeleteAsync(id);
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                BookCount = category.Books?.Count ?? 0,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
