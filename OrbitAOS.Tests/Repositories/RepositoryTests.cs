// ============================================================
// RepositoryTests.cs – Unit Test Project
// Integration tests for the generic Repository<T> using
// EF Core InMemory provider (no real SQL Server required).
//
// Framework: xUnit + FluentAssertions + EF Core InMemory
// ============================================================

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Common;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Infrastructure.Repositories;
using Xunit;

namespace OrbitAOS.Tests.Repositories
{
    // --------------------------------------------------------
    // Minimal test entity – only used in this test file
    // --------------------------------------------------------
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class RepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Repository<TestEntity> _repository;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new Repository<TestEntity>(_context);
        }

        [Fact]
        public async Task AddAsync_And_GetByIdAsync_ReturnsEntity()
        {
            // Arrange
            var entity = new TestEntity { Name = "Test" };

            // Act
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            var retrieved = await _repository.GetByIdAsync(entity.Id);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Test");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            await _repository.AddAsync(new TestEntity { Name = "A" });
            await _repository.AddAsync(new TestEntity { Name = "B" });
            await _repository.SaveChangesAsync();

            // Act
            var all = await _repository.GetAllAsync();

            // Assert
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task Delete_RemovesEntity()
        {
            // Arrange
            var entity = new TestEntity { Name = "ToDelete" };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Act
            _repository.Delete(entity);
            await _repository.SaveChangesAsync();
            var retrieved = await _repository.GetByIdAsync(entity.Id);

            // Assert
            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task Update_ChangesEntityValues()
        {
            // Arrange
            var entity = new TestEntity { Name = "Original" };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Act
            entity.Name = "Updated";
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            var retrieved = await _repository.GetByIdAsync(entity.Id);

            // Assert
            retrieved!.Name.Should().Be("Updated");
            retrieved.UpdatedAt.Should().NotBeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
