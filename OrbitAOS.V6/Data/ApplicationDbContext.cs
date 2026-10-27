using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrbitAOS.V6.Data
{
    /// <summary>
    /// Application database context.
    /// Inherits from IdentityDbContext to include all ASP.NET Core Identity tables
    /// (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.).
    ///
    /// Upgraded from EF Core 6 to EF Core 8 (Microsoft.EntityFrameworkCore.SqlServer 8.0.0).
    /// No breaking API changes for this base class between EF Core 6 and EF Core 8.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your application-specific DbSet<T> properties here, for example:
        // public DbSet<Product> Products { get; set; }
    }
}
