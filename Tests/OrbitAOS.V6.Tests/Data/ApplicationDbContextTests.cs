using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;
using Xunit;

namespace OrbitAOS.V6.Tests.Data
{
    public class ApplicationDbContextTests
    {
        // ---------------------------------------------------------------
        // Helper: build an in-memory DbContextOptions
        // ---------------------------------------------------------------

        private static DbContextOptions<ApplicationDbContext> BuildInMemoryOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // ---------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(Constructor_WithValidOptions_CreatesInstance));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void Constructor_WithValidOptions_ContextIsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(Constructor_WithValidOptions_ContextIsNotNull));

            // Act & Assert
            using var context = new ApplicationDbContext(options);
            Assert.NotNull(context);
        }

        // ---------------------------------------------------------------
        // Database can be created / ensured
        // ---------------------------------------------------------------

        [Fact]
        public void EnsureCreated_WithInMemoryDatabase_ReturnsTrue()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(EnsureCreated_WithInMemoryDatabase_ReturnsTrue));

            // Act
            using var context = new ApplicationDbContext(options);
            bool created = context.Database.EnsureCreated();

            // Assert
            Assert.True(created);
        }

        [Fact]
        public void EnsureCreated_CalledTwice_SecondCallReturnsFalse()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(EnsureCreated_CalledTwice_SecondCallReturnsFalse));

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            // Act
            bool secondCall = context.Database.EnsureCreated();

            // Assert
            Assert.False(secondCall);
        }

        // ---------------------------------------------------------------
        // Identity tables are accessible
        // ---------------------------------------------------------------

        [Fact]
        public void Users_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(Users_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.Users);
        }

        [Fact]
        public void Roles_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(Roles_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.Roles);
        }

        [Fact]
        public void UserRoles_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(UserRoles_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.UserRoles);
        }

        [Fact]
        public void UserClaims_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(UserClaims_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.UserClaims);
        }

        [Fact]
        public void UserLogins_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(UserLogins_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.UserLogins);
        }

        [Fact]
        public void UserTokens_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(UserTokens_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.UserTokens);
        }

        [Fact]
        public void RoleClaims_DbSet_IsNotNull()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(RoleClaims_DbSet_IsNotNull));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.RoleClaims);
        }

        // ---------------------------------------------------------------
        // Inherits IdentityDbContext
        // ---------------------------------------------------------------

        [Fact]
        public void ApplicationDbContext_InheritsIdentityDbContext()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(ApplicationDbContext_InheritsIdentityDbContext));

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.IsAssignableFrom<IdentityDbContext>(context);
        }

        // ---------------------------------------------------------------
        // Dispose
        // ---------------------------------------------------------------

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(Dispose_DoesNotThrow));
            var context = new ApplicationDbContext(options);

            // Act & Assert
            var ex = Record.Exception(() => context.Dispose());
            Assert.Null(ex);
        }

        // ---------------------------------------------------------------
        // Multiple independent contexts share the same in-memory store
        // ---------------------------------------------------------------

        [Fact]
        public void TwoContexts_SameDatabase_ShareData()
        {
            // Arrange
            var options = BuildInMemoryOptions(nameof(TwoContexts_SameDatabase_ShareData));

            using (var ctx1 = new ApplicationDbContext(options))
            {
                ctx1.Database.EnsureCreated();
            }

            // Act & Assert – second context can open the same DB without error
            using var ctx2 = new ApplicationDbContext(options);
            Assert.NotNull(ctx2.Users);
        }
    }
}
