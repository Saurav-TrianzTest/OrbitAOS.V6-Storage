// ============================================================
// ApplicationDbContextFactory.cs – Infrastructure Layer
// IDesignTimeDbContextFactory implementation.
//
// Required for EF Core CLI tools (dotnet ef migrations add, etc.)
// when the DbContext is in a different project than the startup
// project. Without this, dotnet ef cannot instantiate the context.
//
// Usage:
//   dotnet ef migrations add <Name> \
//     --project OrbitAOS.Infrastructure \
//     --startup-project OrbitAOS.V6
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrbitAOS.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for ApplicationDbContext.
    /// Used exclusively by EF Core CLI tools; not used at runtime.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration from the Web project's appsettings.json
            // The working directory during design-time is the startup project folder.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. " +
                    "Ensure appsettings.json is present in the startup project directory.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
