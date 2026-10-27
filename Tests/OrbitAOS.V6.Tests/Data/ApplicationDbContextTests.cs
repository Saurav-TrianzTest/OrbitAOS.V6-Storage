using Xunit;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OrbitAOS.V6.Data;

namespace OrbitAOS.V6.Tests.Data
{
    /// <summary>
    /// Unit tests for ApplicationDbContext.
    /// Uses the EF Core InMemory provider to avoid requiring a real database.
    /// </summary>
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ApplicationDbContextTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        public void Dispose()
        {
            // Each test gets its own in-memory database (unique name), so no explicit cleanup needed.
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Act
            using var context = new ApplicationDbContext(_options);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ApplicationDbContext(null!);
            });
        }

        // -----------------------------------------------------------------------
        // Database can be created / ensured
        // -----------------------------------------------------------------------

        [Fact]
        public void EnsureCreated_WithInMemoryProvider_ReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act
            var created = context.Database.EnsureCreated();

            // Assert
            Assert.True(created);
        }

        [Fact]
        public void EnsureCreated_CalledTwice_SecondCallReturnsFalse()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act
            context.Database.EnsureCreated();
            var secondCall = context.Database.EnsureCreated();

            // Assert – database already exists on second call
            Assert.False(secondCall);
        }

        // -----------------------------------------------------------------------
        // Identity tables are accessible via inherited DbSets
        // -----------------------------------------------------------------------

        [Fact]
        public void Users_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.Users);
        }

        [Fact]
        public void Roles_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.Roles);
        }

        [Fact]
        public void UserClaims_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.UserClaims);
        }

        [Fact]
        public void UserRoles_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.UserRoles);
        }

        [Fact]
        public void UserLogins_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.UserLogins);
        }

        [Fact]
        public void UserTokens_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act & Assert
            Assert.NotNull(context.UserTokens);
        }

        // -----------------------------------------------------------------------
        // CRUD operations on IdentityUser via InMemory provider
        // -----------------------------------------------------------------------

        [Fact]
        public void AddUser_AndSaveChanges_UserIsPersisted()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();

            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "testuser@example.com",
                NormalizedUserName = "TESTUSER@EXAMPLE.COM",
                Email = "testuser@example.com",
                NormalizedEmail = "TESTUSER@EXAMPLE.COM",
                EmailConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Act
            context.Users.Add(user);
            var saved = context.SaveChanges();

            // Assert
            Assert.Equal(1, saved);
        }

        [Fact]
        public void AddUser_ThenFind_ReturnsCorrectUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();

            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser
            {
                Id = userId,
                UserName = "findme@example.com",
                NormalizedUserName = "FINDME@EXAMPLE.COM",
                Email = "findme@example.com",
                NormalizedEmail = "FINDME@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            context.Users.Add(user);
            context.SaveChanges();

            // Act
            var found = context.Users.Find(userId);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("findme@example.com", found!.UserName);
        }

        [Fact]
        public void AddRole_AndSaveChanges_RoleIsPersisted()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // Act
            context.Roles.Add(role);
            var saved = context.SaveChanges();

            // Assert
            Assert.Equal(1, saved);
        }

        [Fact]
        public void RemoveUser_AndSaveChanges_UserIsDeleted()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();

            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser
            {
                Id = userId,
                UserName = "delete@example.com",
                NormalizedUserName = "DELETE@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            context.Users.Add(user);
            context.SaveChanges();

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            // Assert
            var found = context.Users.Find(userId);
            Assert.Null(found);
        }

        // -----------------------------------------------------------------------
        // Multiple contexts with same options share the same in-memory store
        // -----------------------------------------------------------------------

        [Fact]
        public void TwoContextsWithSameOptions_ShareData()
        {
            // Arrange
            var sharedOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("shared-db-test")
                .Options;

            var userId = Guid.NewGuid().ToString();

            using (var ctx1 = new ApplicationDbContext(sharedOptions))
            {
                ctx1.Database.EnsureCreated();
                ctx1.Users.Add(new IdentityUser
                {
                    Id = userId,
                    UserName = "shared@example.com",
                    NormalizedUserName = "SHARED@EXAMPLE.COM",
                    SecurityStamp = Guid.NewGuid().ToString()
                });
                ctx1.SaveChanges();
            }

            // Act
            using var ctx2 = new ApplicationDbContext(sharedOptions);
            var found = ctx2.Users.Find(userId);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("shared@example.com", found!.UserName);
        }

        // -----------------------------------------------------------------------
        // OnModelCreating – verify model is built without exceptions
        // -----------------------------------------------------------------------

        [Fact]
        public void OnModelCreating_DoesNotThrow()
        {
            // Arrange & Act
            Exception? caughtException = null;
            try
            {
                using var context = new ApplicationDbContext(_options);
                // Accessing the model forces OnModelCreating to run
                var model = context.Model;
                Assert.NotNull(model);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.Null(caughtException);
        }

        [Fact]
        public void Model_ContainsIdentityUserEntityType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(IdentityUser));

            // Assert
            Assert.NotNull(entityType);
        }

        [Fact]
        public void Model_ContainsIdentityRoleEntityType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(IdentityRole));

            // Assert
            Assert.NotNull(entityType);
        }
    }
}
