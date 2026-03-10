using Xunit;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OrbitAOS.V6.Data.Tests
{
    public class ApplicationDbContextTests
    {
        private DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WithValidOptions()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenOptionsIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApplicationDbContext(null!));
        }

        [Fact]
        public void ApplicationDbContext_ShouldInheritFromIdentityDbContext()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.IsAssignableFrom<IdentityDbContext>(context);
        }

        [Fact]
        public void Database_ShouldBeAccessible()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var database = context.Database;

            // Assert
            Assert.NotNull(database);
        }

        [Fact]
        public void Context_ShouldHaveIdentityUserTable()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(IdentityUser));

            // Assert
            Assert.NotNull(entityType);
        }

        [Fact]
        public void Context_ShouldHaveIdentityRoleTable()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(IdentityRole));

            // Assert
            Assert.NotNull(entityType);
        }

        [Fact]
        public void SaveChanges_ShouldWorkWithoutError()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var result = context.SaveChanges();

            // Assert
            Assert.Equal(0, result); // No changes made
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldWorkWithoutError()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var result = await context.SaveChangesAsync();

            // Assert
            Assert.Equal(0, result); // No changes made
        }

        [Fact]
        public void CanCreateMultipleContexts_WithDifferentOptions()
        {
            // Arrange
            var options1 = CreateInMemoryOptions();
            var options2 = CreateInMemoryOptions();

            // Act
            using var context1 = new ApplicationDbContext(options1);
            using var context2 = new ApplicationDbContext(options2);

            // Assert
            Assert.NotNull(context1);
            Assert.NotNull(context2);
            Assert.NotSame(context1, context2);
        }

        [Fact]
        public void Context_ShouldBeDisposable()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var context = new ApplicationDbContext(options);

            // Act & Assert (no exception should be thrown)
            context.Dispose();
        }

        [Fact]
        public void Model_ShouldNotBeNull()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var model = context.Model;

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Context_ShouldAllowQueryingUsers()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var users = context.Users.ToList();

            // Assert
            Assert.NotNull(users);
            Assert.Empty(users); // Should be empty initially
        }

        [Fact]
        public void Context_ShouldAllowQueryingRoles()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var roles = context.Roles.ToList();

            // Assert
            Assert.NotNull(roles);
            Assert.Empty(roles); // Should be empty initially
        }

        [Fact]
        public async Task Context_ShouldSupportAsyncOperations()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var users = await context.Users.ToListAsync();

            // Assert
            Assert.NotNull(users);
        }

        [Fact]
        public void OnModelCreating_ShouldExecuteWithoutError()
        {
            // Arrange & Act
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // The model is built during context initialization
            var model = context.Model;

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Context_CanAddAndRetrieveUser()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user = new IdentityUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();
            var retrievedUser = context.Users.FirstOrDefault(u => u.UserName == "testuser");

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("testuser", retrievedUser.UserName);
            Assert.Equal("test@example.com", retrievedUser.Email);
        }

        [Fact]
        public void Context_CanAddAndRetrieveRole()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var role = new IdentityRole
            {
                Name = "Admin",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            context.Roles.Add(role);
            context.SaveChanges();
            var retrievedRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

            // Assert
            Assert.NotNull(retrievedRole);
            Assert.Equal("Admin", retrievedRole.Name);
        }

        [Fact]
        public void Context_ShouldHandleMultipleUsers()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user1 = new IdentityUser { UserName = "user1", Email = "user1@example.com", Id = Guid.NewGuid().ToString() };
            var user2 = new IdentityUser { UserName = "user2", Email = "user2@example.com", Id = Guid.NewGuid().ToString() };
            var user3 = new IdentityUser { UserName = "user3", Email = "user3@example.com", Id = Guid.NewGuid().ToString() };

            // Act
            context.Users.AddRange(user1, user2, user3);
            var result = context.SaveChanges();

            // Assert
            Assert.Equal(3, result);
            Assert.Equal(3, context.Users.Count());
        }

        [Fact]
        public void Context_CanUpdateUser()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user = new IdentityUser { UserName = "user", Email = "old@example.com", Id = Guid.NewGuid().ToString() };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            user.Email = "new@example.com";
            context.SaveChanges();
            var updatedUser = context.Users.First(u => u.UserName == "user");

            // Assert
            Assert.Equal("new@example.com", updatedUser.Email);
        }

        [Fact]
        public void Context_CanDeleteUser()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user = new IdentityUser { UserName = "deleteuser", Email = "delete@example.com", Id = Guid.NewGuid().ToString() };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            // Assert
            Assert.Empty(context.Users);
        }

        [Fact]
        public async Task Context_CanAddUserAsync()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user = new IdentityUser { UserName = "asyncuser", Email = "async@example.com", Id = Guid.NewGuid().ToString() };

            // Act
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            var foundUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "asyncuser");

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal("async@example.com", foundUser.Email);
        }

        [Fact]
        public void Context_ShouldHaveUsersDbSet()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.Users);
        }

        [Fact]
        public void Context_ShouldHaveRolesDbSet()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.Roles);
        }

        [Fact]
        public void Model_ShouldContainIdentityTables()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act
            var entityTypes = context.Model.GetEntityTypes();

            // Assert
            Assert.NotEmpty(entityTypes);
            Assert.Contains(entityTypes, e => e.ClrType == typeof(IdentityUser));
        }

        [Fact]
        public void Context_ShouldSupportLinqQueries()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            var user1 = new IdentityUser { UserName = "alice", Email = "alice@example.com", Id = Guid.NewGuid().ToString() };
            var user2 = new IdentityUser { UserName = "bob", Email = "bob@example.com", Id = Guid.NewGuid().ToString() };
            context.Users.AddRange(user1, user2);
            context.SaveChanges();

            // Act
            var result = context.Users.Where(u => u.UserName!.StartsWith("a")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("alice", result[0].UserName);
        }

        [Fact]
        public void Context_ShouldBeInCorrectNamespace()
        {
            // Arrange
            var contextType = typeof(ApplicationDbContext);

            // Assert
            Assert.Equal("OrbitAOS.V6.Data", contextType.Namespace);
        }

        [Fact]
        public void Context_ShouldBePublicClass()
        {
            // Arrange
            var contextType = typeof(ApplicationDbContext);

            // Assert
            Assert.True(contextType.IsPublic);
            Assert.True(contextType.IsClass);
        }

        [Fact]
        public void Context_Constructor_ShouldAcceptDbContextOptions()
        {
            // Arrange
            var constructorInfo = typeof(ApplicationDbContext).GetConstructors()[0];
            var parameters = constructorInfo.GetParameters();

            // Assert
            Assert.Single(parameters);
            Assert.Equal(typeof(DbContextOptions<ApplicationDbContext>), parameters[0].ParameterType);
        }

        [Fact]
        public void Context_ShouldHaveOnModelCreatingMethod()
        {
            // Arrange
            var methodInfo = typeof(ApplicationDbContext).GetMethod("OnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
        }

        [Fact]
        public void Context_ShouldHaveOnConfiguringMethod()
        {
            // Arrange
            var methodInfo = typeof(ApplicationDbContext).GetMethod("OnConfiguring",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
        }
    }
}
