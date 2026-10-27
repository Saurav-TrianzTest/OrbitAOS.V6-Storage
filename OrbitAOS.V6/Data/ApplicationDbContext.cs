using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrbitAOS.V6.Data
{
    /// <summary>
    /// Application database context.
    /// Inherits from IdentityDbContext to include all ASP.NET Core Identity tables
    /// (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.).
    ///
    /// Migrated from SQL Server (EF Core 8 SqlServer provider) to PostgreSQL
    /// (Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0).
    /// Snake_case naming convention is applied via UseSnakeCaseNamingConvention()
    /// configured in Program.cs.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set default schema to "public" for PostgreSQL
            builder.HasDefaultSchema("public");

            // Configure PostgreSQL extensions
            builder.HasPostgresExtension("uuid-ossp");
            builder.HasPostgresExtension("citext");

            // Configure snake_case index filters for PostgreSQL compatibility
            // PostgreSQL uses double-quoted identifiers; remove SQL Server IS NOT NULL filter syntax
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity =>
            {
                entity.HasIndex(r => r.NormalizedName)
                    .HasDatabaseName("role_name_index")
                    .IsUnique();
            });

            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUser>(entity =>
            {
                entity.HasIndex(u => u.NormalizedEmail)
                    .HasDatabaseName("email_index");

                entity.HasIndex(u => u.NormalizedUserName)
                    .HasDatabaseName("user_name_index")
                    .IsUnique();
            });
        }

        // Add your application-specific DbSet<T> properties here, for example:
        // public DbSet<Product> Products { get; set; }
    }
}
