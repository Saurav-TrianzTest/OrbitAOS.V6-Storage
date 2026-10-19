// ============================================================
// ApplicationDbContext.cs – Infrastructure Layer
// EF Core DbContext for the OrbitAOS application.
//
// Migration from net6.0 → net10.0:
//   - Package versions updated to 10.0.0
//   - Moved from OrbitAOS.V6/Data/ to OrbitAOS.Infrastructure/Data/
//   - Inherits IdentityDbContext<IdentityUser> (unchanged)
//   - OnModelCreating extended for domain entity configurations
//
// Migration notes:
//   - EF Core 10 removes deprecated HasName() on indexes;
//     use HasDatabaseName() instead (see migration file fix below)
//   - nullable: false columns now use IsRequired() not nullable: false
//     in column definitions
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrbitAOS.Infrastructure.Data
{
    /// <summary>
    /// Primary EF Core DbContext.
    /// Inherits from IdentityDbContext to include ASP.NET Core Identity tables.
    /// Add DbSet&lt;T&gt; properties for each domain entity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --------------------------------------------------------
        // Add domain entity DbSets here as the application grows.
        // Example:
        //   public DbSet<Order> Orders => Set<Order>();
        //   public DbSet<Product> Products => Set<Product>();
        // --------------------------------------------------------

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration<T> classes from this assembly.
            // Place entity configurations in Data/Configurations/ folder.
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
