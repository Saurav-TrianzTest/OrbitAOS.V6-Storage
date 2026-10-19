// ============================================================
// ApplicationDbContextModelSnapshot.cs – Infrastructure Layer
// EF Core model snapshot for ApplicationDbContext.
//
// Migration from net6.0 → net10.0 fixes:
//   1. Namespace updated to OrbitAOS.Infrastructure.Data.Migrations
//   2. ProductVersion updated to reflect EF Core 10
//   3. Deprecated HasName() replaced with HasDatabaseName()
//   4. HasMaxLength() used instead of deprecated .HasMaxLength()
//      chained on HasColumnType()
//   5. 'using System;' removed – covered by ImplicitUsings
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrbitAOS.Infrastructure.Data;

namespace OrbitAOS.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy",
                    SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
            {
                b.Property<string>("Id").HasColumnType("nvarchar(450)");
                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("nvarchar(max)");
                b.Property<string>("Name")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.Property<string>("NormalizedName")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.HasKey("Id");
                b.HasIndex("NormalizedName")
                    .IsUnique()
                    .HasDatabaseName("RoleNameIndex")   // EF Core 10: HasName() → HasDatabaseName()
                    .HasFilter("[NormalizedName] IS NOT NULL");
                b.ToTable("AspNetRoles");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy",
                        SqlServerValueGenerationStrategy.IdentityColumn);
                b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
                b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
                b.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("Id");
                b.HasIndex("RoleId");
                b.ToTable("AspNetRoleClaims");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
            {
                b.Property<string>("Id").HasColumnType("nvarchar(450)");
                b.Property<int>("AccessFailedCount").HasColumnType("int");
                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("nvarchar(max)");
                b.Property<string>("Email")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.Property<bool>("EmailConfirmed").HasColumnType("bit");
                b.Property<bool>("LockoutEnabled").HasColumnType("bit");
                b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
                b.Property<string>("NormalizedEmail")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.Property<string>("NormalizedUserName")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
                b.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
                b.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");
                b.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
                b.Property<bool>("TwoFactorEnabled").HasColumnType("bit");
                b.Property<string>("UserName")
                    .HasColumnType("nvarchar(256)")
                    .HasMaxLength(256);
                b.HasKey("Id");
                b.HasIndex("NormalizedEmail")
                    .HasDatabaseName("EmailIndex");     // EF Core 10: HasName() → HasDatabaseName()
                b.HasIndex("NormalizedUserName")
                    .IsUnique()
                    .HasDatabaseName("UserNameIndex")   // EF Core 10: HasName() → HasDatabaseName()
                    .HasFilter("[NormalizedUserName] IS NOT NULL");
                b.ToTable("AspNetUsers");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy",
                        SqlServerValueGenerationStrategy.IdentityColumn);
                b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
                b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
                b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("Id");
                b.HasIndex("UserId");
                b.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(128)")
                    .HasMaxLength(128);
                b.Property<string>("ProviderKey")
                    .HasColumnType("nvarchar(128)")
                    .HasMaxLength(128);
                b.Property<string>("ProviderDisplayName").HasColumnType("nvarchar(max)");
                b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("LoginProvider", "ProviderKey");
                b.HasIndex("UserId");
                b.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.Property<string>("UserId").HasColumnType("nvarchar(450)");
                b.Property<string>("RoleId").HasColumnType("nvarchar(450)");
                b.HasKey("UserId", "RoleId");
                b.HasIndex("RoleId");
                b.ToTable("AspNetUserRoles");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.Property<string>("UserId").HasColumnType("nvarchar(450)");
                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(128)")
                    .HasMaxLength(128);
                b.Property<string>("Name")
                    .HasColumnType("nvarchar(128)")
                    .HasMaxLength(128);
                b.Property<string>("Value").HasColumnType("nvarchar(max)");
                b.HasKey("UserId", "LoginProvider", "Name");
                b.ToTable("AspNetUserTokens");
            });

            // Relationship configurations
            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
#pragma warning restore 612, 618
        }
    }
}
