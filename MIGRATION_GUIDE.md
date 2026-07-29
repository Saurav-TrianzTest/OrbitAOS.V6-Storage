# ASP.NET MVC to .NET 8 Core MVC Migration Guide
## Clean Architecture Implementation

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Migration Overview](#migration-overview)
3. [Architecture Transformation](#architecture-transformation)
4. [Step-by-Step Migration Plan](#step-by-step-migration-plan)
5. [Folder-by-Folder Mapping](#folder-by-folder-mapping)
6. [Transformation Rules (30+)](#transformation-rules)
7. [Data Strategy with EF Core](#data-strategy-with-ef-core)
8. [Authentication Migration](#authentication-migration)
9. [Blockers & Mitigations (15+)](#blockers-and-mitigations)
10. [Unit Testing Framework](#unit-testing-framework)
11. [Execution Plan JSON](#execution-plan-json)

---

## Executive Summary

This document provides a comprehensive guide for migrating the OrbitAOS.V6 application from ASP.NET MVC on .NET 6 to ASP.NET Core MVC on .NET 8 with Clean Architecture implementation.

### Key Achievements
- ✅ Upgraded from .NET 6 to .NET 8
- ✅ Implemented Clean Architecture (Domain/Application/Infrastructure/Web)
- ✅ Repository Pattern and Unit of Work
- ✅ Dependency Injection throughout all layers
- ✅ Modern async/await patterns
- ✅ Entity Framework Core 8.0
- ✅ ASP.NET Core Identity 8.0

### Migration Complexity: **MEDIUM**
- **Estimated Effort**: 40-60 hours
- **Team Size**: 2-3 developers
- **Timeline**: 2-3 weeks

---

## Migration Overview

### Source Application
- **Framework**: ASP.NET MVC on .NET 6
- **Architecture**: Monolithic MVC
- **Data Access**: Entity Framework Core 6.0
- **Authentication**: ASP.NET Core Identity 6.0

### Target Application
- **Framework**: ASP.NET Core MVC on .NET 8
- **Architecture**: Clean Architecture (4 layers)
- **Data Access**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity 8.0

---

## Architecture Transformation

### Before: Monolithic MVC Structure
```
OrbitAOS.V6/
├── Controllers/
├── Views/
├── Models/
├── Data/
├── wwwroot/
├── Program.cs
└── OrbitAOS.V6.csproj
```

### After: Clean Architecture Structure
```
OrbitAOS.V6/
├── OrbitAOS.V6.Domain/          # Enterprise business rules
│   ├── Entities/
│   └── Common/
│
├── OrbitAOS.V6.Application/     # Application business rules
│   ├── Interfaces/
│   ├── Services/
│   ├── DTOs/
│   └── Common/
│
├── OrbitAOS.V6.Infrastructure/  # External concerns
│   ├── Data/
│   ├── Identity/
│   └── Services/
│
└── OrbitAOS.V6.Web/            # Presentation layer
    ├── Controllers/
    ├── Views/
    ├── Models/
    ├── wwwroot/
    └── Program.cs
```

### Dependency Flow
```
Web → Application → Domain
  ↓
Infrastructure → Application → Domain
```

**Key Principle**: Dependencies point inward. Domain has no dependencies.

---

## Step-by-Step Migration Plan

### Phase 1: Pre-Migration Assessment (Week 1, Days 1-2)

#### Tasks:
1. **Inventory Current Application**
   - Document all controllers, views, models
   - List all NuGet packages and versions
   - Identify custom middleware and filters
   - Document database schema

2. **Identify Dependencies**
   - Third-party libraries
   - Custom components
   - External services
   - API integrations

3. **Risk Assessment**
   - Breaking changes in .NET 8
   - Package compatibility
   - Custom code that needs refactoring

#### Deliverables:
- Application inventory document
- Dependency matrix
- Risk assessment report

---

### Phase 2: Environment Setup (Week 1, Days 3-4)

#### Tasks:
1. **Install Required Tools**
   ```bash
   # Install .NET 8 SDK
   winget install Microsoft.DotNet.SDK.8
   
   # Install EF Core tools
   dotnet tool install --global dotnet-ef --version 8.0.0
   
   # Verify installation
   dotnet --version  # Should show 8.0.x
   dotnet ef --version  # Should show 8.0.x
   ```

2. **Create Solution Structure**
   ```bash
   # Create solution
   dotnet new sln -n OrbitAOS.V6
   
   # Create projects
   dotnet new classlib -n OrbitAOS.V6.Domain -f net8.0
   dotnet new classlib -n OrbitAOS.V6.Application -f net8.0
   dotnet new classlib -n OrbitAOS.V6.Infrastructure -f net8.0
   dotnet new mvc -n OrbitAOS.V6.Web -f net8.0
   
   # Add projects to solution
   dotnet sln add OrbitAOS.V6.Domain/OrbitAOS.V6.Domain.csproj
   dotnet sln add OrbitAOS.V6.Application/OrbitAOS.V6.Application.csproj
   dotnet sln add OrbitAOS.V6.Infrastructure/OrbitAOS.V6.Infrastructure.csproj
   dotnet sln add OrbitAOS.V6.Web/OrbitAOS.V6.Web.csproj
   
   # Add project references
   cd OrbitAOS.V6.Application
   dotnet add reference ../OrbitAOS.V6.Domain/OrbitAOS.V6.Domain.csproj
   
   cd ../OrbitAOS.V6.Infrastructure
   dotnet add reference ../OrbitAOS.V6.Domain/OrbitAOS.V6.Domain.csproj
   dotnet add reference ../OrbitAOS.V6.Application/OrbitAOS.V6.Application.csproj
   
   cd ../OrbitAOS.V6.Web
   dotnet add reference ../OrbitAOS.V6.Application/OrbitAOS.V6.Application.csproj
   dotnet add reference ../OrbitAOS.V6.Infrastructure/OrbitAOS.V6.Infrastructure.csproj
   ```

3. **Install NuGet Packages**
   ```bash
   # Infrastructure layer
   cd OrbitAOS.V6.Infrastructure
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
   dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
   
   # Web layer
   cd ../OrbitAOS.V6.Web
   dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 8.0.0
   dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
   ```

#### Deliverables:
- Configured development environment
- Solution structure created
- All dependencies installed

---

### Phase 3: Domain Layer Migration (Week 1, Day 5)

#### Tasks:
1. **Create Base Entity**
   ```csharp
   // OrbitAOS.V6.Domain/Common/BaseEntity.cs
   public abstract class BaseEntity
   {
       public int Id { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }
       public string? CreatedBy { get; set; }
       public string? UpdatedBy { get; set; }
   }
   ```

2. **Migrate Domain Entities**
   - Move entities from old Models folder
   - Inherit from BaseEntity
   - Remove data annotations (move to Infrastructure)
   - Keep only business logic

3. **Create Domain Interfaces** (if any)
   - Domain services
   - Domain events
   - Specifications

#### Deliverables:
- Domain layer with all entities
- Base classes and common logic
- No external dependencies

---

### Phase 4: Application Layer Migration (Week 2, Days 1-2)

#### Tasks:
1. **Create Repository Interfaces**
   ```csharp
   // OrbitAOS.V6.Application/Interfaces/IRepository.cs
   public interface IRepository<T> where T : class
   {
       Task<T?> GetByIdAsync(int id);
       Task<IEnumerable<T>> GetAllAsync();
       Task<T> AddAsync(T entity);
       Task UpdateAsync(T entity);
       Task DeleteAsync(int id);
   }
   ```

2. **Create Service Interfaces**
   ```csharp
   // OrbitAOS.V6.Application/Interfaces/IProductService.cs
   public interface IProductService
   {
       Task<IEnumerable<ProductDto>> GetAllAsync();
       Task<ProductDto?> GetByIdAsync(int id);
       Task<ProductDto> CreateAsync(ProductDto dto);
       Task UpdateAsync(int id, ProductDto dto);
       Task DeleteAsync(int id);
   }
   ```

3. **Create DTOs**
   ```csharp
   // OrbitAOS.V6.Application/DTOs/ProductDto.cs
   public class ProductDto
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal Price { get; set; }
   }
   ```

4. **Implement Services**
   ```csharp
   // OrbitAOS.V6.Application/Services/ProductService.cs
   public class ProductService : IProductService
   {
       private readonly IRepository<Product> _repository;
       private readonly IUnitOfWork _unitOfWork;
       
       public ProductService(
           IRepository<Product> repository,
           IUnitOfWork unitOfWork)
       {
           _repository = repository;
           _unitOfWork = unitOfWork;
       }
       
       public async Task<IEnumerable<ProductDto>> GetAllAsync()
       {
           var products = await _repository.GetAllAsync();
           return products.Select(MapToDto);
       }
       
       // Other methods...
   }
   ```

5. **Configure Dependency Injection**
   ```csharp
   // OrbitAOS.V6.Application/Common/DependencyInjection.cs
   public static class DependencyInjection
   {
       public static IServiceCollection AddApplicationServices(
           this IServiceCollection services)
       {
           services.AddScoped<IProductService, ProductService>();
           // Add more services...
           return services;
       }
   }
   ```

#### Deliverables:
- All service interfaces defined
- DTOs created
- Service implementations
- DI configuration

---

### Phase 5: Infrastructure Layer Migration (Week 2, Days 3-4)

#### Tasks:
1. **Create DbContext**
   ```csharp
   // OrbitAOS.V6.Infrastructure/Data/ApplicationDbContext.cs
   public class ApplicationDbContext : IdentityDbContext
   {
       public ApplicationDbContext(
           DbContextOptions<ApplicationDbContext> options)
           : base(options)
       {
       }
       
       public DbSet<Product> Products { get; set; }
       
       protected override void OnModelCreating(ModelBuilder builder)
       {
           base.OnModelCreating(builder);
           
           builder.Entity<Product>(entity =>
           {
               entity.HasKey(e => e.Id);
               entity.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(200);
               entity.Property(e => e.Price)
                   .HasColumnType("decimal(18,2)");
           });
       }
   }
   ```

2. **Implement Repository**
   ```csharp
   // OrbitAOS.V6.Infrastructure/Data/Repository.cs
   public class Repository<T> : IRepository<T> where T : class
   {
       protected readonly ApplicationDbContext _context;
       protected readonly DbSet<T> _dbSet;
       
       public Repository(ApplicationDbContext context)
       {
           _context = context;
           _dbSet = context.Set<T>();
       }
       
       public virtual async Task<T?> GetByIdAsync(int id)
       {
           return await _dbSet.FindAsync(id);
       }
       
       // Other methods...
   }
   ```

3. **Implement Unit of Work**
   ```csharp
   // OrbitAOS.V6.Infrastructure/Data/UnitOfWork.cs
   public class UnitOfWork : IUnitOfWork
   {
       private readonly ApplicationDbContext _context;
       
       public UnitOfWork(ApplicationDbContext context)
       {
           _context = context;
       }
       
       public async Task<int> SaveChangesAsync(
           CancellationToken cancellationToken = default)
       {
           return await _context.SaveChangesAsync(cancellationToken);
       }
       
       // Transaction methods...
   }
   ```

4. **Configure Infrastructure DI**
   ```csharp
   // OrbitAOS.V6.Infrastructure/DependencyInjection.cs
   public static class DependencyInjection
   {
       public static IServiceCollection AddInfrastructureServices(
           this IServiceCollection services,
           IConfiguration configuration)
       {
           var connectionString = configuration
               .GetConnectionString("DefaultConnection");
           
           services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connectionString));
           
           services.AddDefaultIdentity<IdentityUser>(options =>
           {
               options.SignIn.RequireConfirmedAccount = true;
           })
           .AddEntityFrameworkStores<ApplicationDbContext>();
           
           services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
           services.AddScoped<IUnitOfWork, UnitOfWork>();
           
           return services;
       }
   }
   ```

#### Deliverables:
- DbContext configured
- Repository implementation
- Unit of Work implementation
- Infrastructure DI setup

---

### Phase 6: Web Layer Migration (Week 2, Day 5)

#### Tasks:
1. **Update Program.cs**
   ```csharp
   // OrbitAOS.V6.Web/Program.cs
   using OrbitAOS.V6.Application.Common;
   using OrbitAOS.V6.Infrastructure;
   
   var builder = WebApplication.CreateBuilder(args);
   
   // Add services
   builder.Services.AddInfrastructureServices(builder.Configuration);
   builder.Services.AddApplicationServices();
   builder.Services.AddDatabaseDeveloperPageExceptionFilter();
   builder.Services.AddControllersWithViews();
   builder.Services.AddRazorPages();
   
   var app = builder.Build();
   
   // Configure pipeline
   if (app.Environment.IsDevelopment())
   {
       app.UseMigrationsEndPoint();
   }
   else
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }
   
   app.UseHttpsRedirection();
   app.UseStaticFiles();
   app.UseRouting();
   app.UseAuthentication();
   app.UseAuthorization();
   
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   app.MapRazorPages();
   
   app.Run();
   ```

2. **Migrate Controllers**
   ```csharp
   // Before (old MVC)
   public class ProductController : Controller
   {
       private readonly ApplicationDbContext _context;
       
       public ProductController(ApplicationDbContext context)
       {
           _context = context;
       }
       
       public ActionResult Index()
       {
           var products = _context.Products.ToList();
           return View(products);
       }
   }
   
   // After (Clean Architecture)
   public class ProductController : Controller
   {
       private readonly IProductService _productService;
       private readonly ILogger<ProductController> _logger;
       
       public ProductController(
           IProductService productService,
           ILogger<ProductController> logger)
       {
           _productService = productService;
           _logger = logger;
       }
       
       public async Task<IActionResult> Index()
       {
           try
           {
               var products = await _productService.GetAllAsync();
               return View(products);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving products");
               return View("Error");
           }
       }
   }
   ```

3. **Copy Views and Static Files**
   ```bash
   # Copy views
   cp -r ../OrbitAOS.V6.Old/Views ./Views
   
   # Copy wwwroot
   cp -r ../OrbitAOS.V6.Old/wwwroot ./wwwroot
   
   # Copy Areas (if any)
   cp -r ../OrbitAOS.V6.Old/Areas ./Areas
   ```

4. **Update View Imports**
   ```cshtml
   @* Views/_ViewImports.cshtml *@
   @using OrbitAOS.V6.Web
   @using OrbitAOS.V6.Web.Models
   @using OrbitAOS.V6.Application.DTOs
   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   ```

#### Deliverables:
- Program.cs configured
- All controllers migrated
- Views and static files copied
- Application running

---

### Phase 7: Database Migration (Week 3, Day 1)

#### Tasks:
1. **Create Initial Migration**
   ```bash
   cd OrbitAOS.V6.Web
   dotnet ef migrations add InitialCreate \
       --project ../OrbitAOS.V6.Infrastructure \
       --startup-project . \
       --context ApplicationDbContext \
       --output-dir Data/Migrations
   ```

2. **Review Migration**
   - Check generated migration files
   - Verify Up() and Down() methods
   - Ensure all entities are included

3. **Update Database**
   ```bash
   dotnet ef database update \
       --project ../OrbitAOS.V6.Infrastructure \
       --startup-project .
   ```

4. **Verify Database**
   ```sql
   -- Connect to database and verify tables
   SELECT * FROM INFORMATION_SCHEMA.TABLES
   WHERE TABLE_TYPE = 'BASE TABLE'
   ```

#### Deliverables:
- Database migrations created
- Database updated
- Schema verified

---

### Phase 8: Testing (Week 3, Days 2-3)

#### Tasks:
1. **Create Test Projects**
   ```bash
   dotnet new xunit -n OrbitAOS.V6.Application.Tests
   dotnet new xunit -n OrbitAOS.V6.Infrastructure.Tests
   dotnet new xunit -n OrbitAOS.V6.Web.Tests
   
   # Add to solution
   dotnet sln add OrbitAOS.V6.Application.Tests
   dotnet sln add OrbitAOS.V6.Infrastructure.Tests
   dotnet sln add OrbitAOS.V6.Web.Tests
   ```

2. **Install Testing Packages**
   ```bash
   cd OrbitAOS.V6.Application.Tests
   dotnet add package Moq --version 4.20.70
   dotnet add package FluentAssertions --version 6.12.0
   
   cd ../OrbitAOS.V6.Web.Tests
   dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
   ```

3. **Write Unit Tests**
   ```csharp
   // OrbitAOS.V6.Application.Tests/Services/ProductServiceTests.cs
   public class ProductServiceTests
   {
       private readonly Mock<IRepository<Product>> _mockRepository;
       private readonly Mock<IUnitOfWork> _mockUnitOfWork;
       private readonly ProductService _service;
       
       public ProductServiceTests()
       {
           _mockRepository = new Mock<IRepository<Product>>();
           _mockUnitOfWork = new Mock<IUnitOfWork>();
           _service = new ProductService(
               _mockRepository.Object,
               _mockUnitOfWork.Object);
       }
       
       [Fact]
       public async Task GetAllAsync_ReturnsAllProducts()
       {
           // Arrange
           var products = new List<Product>
           {
               new Product { Id = 1, Name = "Product 1" },
               new Product { Id = 2, Name = "Product 2" }
           };
           _mockRepository.Setup(r => r.GetAllAsync())
               .ReturnsAsync(products);
           
           // Act
           var result = await _service.GetAllAsync();
           
           // Assert
           result.Should().HaveCount(2);
       }
   }
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

#### Deliverables:
- Test projects created
- Unit tests written
- All tests passing

---

### Phase 9: Build Verification (Week 3, Day 4)

#### Tasks:
1. **Clean Build**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build --configuration Release
   ```

2. **Verify Build Success**
   - Check for compilation errors
   - Verify all projects build
   - Check warnings

3. **Run Application**
   ```bash
   cd OrbitAOS.V6.Web
   dotnet run
   ```

4. **Manual Testing**
   - Test all major features
   - Verify authentication
   - Check database operations
   - Test error handling

#### Deliverables:
- Successful build
- Application running
- Manual testing completed

---

### Phase 10: Documentation and Handover (Week 3, Day 5)

#### Tasks:
1. **Update Documentation**
   - README.md
   - Architecture diagrams
   - API documentation
   - Deployment guide

2. **Create Migration Report**
   - Changes made
   - Issues encountered
   - Solutions implemented
   - Known limitations

3. **Team Training**
   - Clean Architecture overview
   - New project structure
   - Development workflow
   - Testing approach

#### Deliverables:
- Complete documentation
- Migration report
- Team trained

---

## Folder-by-Folder Mapping

### Legacy Structure → Clean Architecture

| Legacy Location | New Location | Notes |
|----------------|--------------|-------|
| `/Models/` (Entities) | `/OrbitAOS.V6.Domain/Entities/` | Domain entities only |
| `/Models/` (ViewModels) | `/OrbitAOS.V6.Web/Models/` | View-specific models |
| `/Models/` (DTOs) | `/OrbitAOS.V6.Application/DTOs/` | Data transfer objects |
| `/Data/ApplicationDbContext.cs` | `/OrbitAOS.V6.Infrastructure/Data/ApplicationDbContext.cs` | DbContext |
| `/Data/Migrations/` | `/OrbitAOS.V6.Infrastructure/Data/Migrations/` | EF migrations |
| `/Controllers/` | `/OrbitAOS.V6.Web/Controllers/` | MVC controllers |
| `/Views/` | `/OrbitAOS.V6.Web/Views/` | Razor views |
| `/wwwroot/` | `/OrbitAOS.V6.Web/wwwroot/` | Static files |
| `/Areas/` | `/OrbitAOS.V6.Web/Areas/` | MVC areas |
| `/Properties/` | `/OrbitAOS.V6.Web/Properties/` | Launch settings |
| `Program.cs` | `/OrbitAOS.V6.Web/Program.cs` | Application entry point |
| `appsettings.json` | `/OrbitAOS.V6.Web/appsettings.json` | Configuration |
| N/A | `/OrbitAOS.V6.Application/Interfaces/` | Service interfaces (new) |
| N/A | `/OrbitAOS.V6.Application/Services/` | Business logic (new) |
| N/A | `/OrbitAOS.V6.Infrastructure/Data/Repository.cs` | Repository pattern (new) |
| N/A | `/OrbitAOS.V6.Infrastructure/Data/UnitOfWork.cs` | Unit of Work (new) |

---

## Transformation Rules

### Rule 1: Target Framework Update
**Legacy (.NET 6):**
```xml
<TargetFramework>net6.0</TargetFramework>
```

**Modern (.NET 8):**
```xml
<TargetFramework>net8.0</TargetFramework>
```

**Explanation**: Update all project files to target .NET 8 framework.

---

### Rule 2: Package Version Updates
**Legacy (.NET 6):**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.19" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.19" />
```

**Modern (.NET 8):**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
```

**Explanation**: All Microsoft packages must be updated to version 8.0.0 for .NET 8 compatibility.

---

### Rule 3: Controller Direct DbContext Access → Service Layer
**Legacy:**
```csharp
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public ActionResult Index()
    {
        var products = _context.Products.ToList();
        return View(products);
    }
}
```

**Modern:**
```csharp
public class ProductController : Controller
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }
}
```

**Explanation**: Controllers should depend on service interfaces, not DbContext directly. This follows Clean Architecture principles.

---

### Rule 4: Synchronous → Asynchronous Operations
**Legacy:**
```csharp
public ActionResult Details(int id)
{
    var product = _context.Products.Find(id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}
```

**Modern:**
```csharp
public async Task<IActionResult> Details(int id)
{
    var product = await _productService.GetByIdAsync(id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}
```

**Explanation**: All I/O operations should be asynchronous for better scalability.

---

### Rule 5: Entity → DTO Mapping
**Legacy:**
```csharp
public ActionResult Create(Product product)
{
    if (ModelState.IsValid)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    return View(product);
}
```

**Modern:**
```csharp
public async Task<IActionResult> Create(ProductDto dto)
{
    if (ModelState.IsValid)
    {
        await _productService.CreateAsync(dto);
        return RedirectToAction("Index");
    }
    return View(dto);
}
```

**Explanation**: Use DTOs for data transfer between layers, not domain entities.

---

### Rule 6: Domain Entity Structure
**Legacy:**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Modern:**
```csharp
// Domain/Entities/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

// Domain/Common/BaseEntity.cs
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

**Explanation**: Domain entities inherit from BaseEntity for common properties.

---

### Rule 7: DbContext Configuration
**Legacy:**
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
}
```

**Modern:**
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
        });
    }
}
```

**Explanation**: Configure entity mappings in OnModelCreating using Fluent API.

---

### Rule 8: Dependency Injection Configuration
**Legacy:**
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Modern:**
```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}

// Program.cs
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
```

**Explanation**: Organize DI configuration by layer using extension methods.

---

### Rule 9: Repository Pattern Implementation
**Legacy:**
```csharp
// Direct DbContext usage in controllers
var products = _context.Products.ToList();
```

**Modern:**
```csharp
// Application/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Infrastructure/Data/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    // Other methods...
}
```

**Explanation**: Implement repository pattern for data access abstraction.

---

### Rule 10: Unit of Work Pattern
**Legacy:**
```csharp
_context.Products.Add(product);
_context.SaveChanges();
```

**Modern:**
```csharp
// Application/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Infrastructure/Data/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    // Transaction methods...
}

// Usage in service
await _repository.AddAsync(entity);
await _unitOfWork.SaveChangesAsync();
```

**Explanation**: Use Unit of Work pattern for transaction management.

---

### Rule 11: Service Layer Implementation
**Legacy:**
```csharp
// Business logic in controller
public ActionResult Create(Product product)
{
    if (string.IsNullOrEmpty(product.Name))
    {
        ModelState.AddModelError("Name", "Name is required");
        return View(product);
    }
    
    product.CreatedAt = DateTime.UtcNow;
    _context.Products.Add(product);
    _context.SaveChanges();
    
    return RedirectToAction("Index");
}
```

**Modern:**
```csharp
// Application/Interfaces/IProductService.cs
public interface IProductService
{
    Task<ProductDto> CreateAsync(ProductDto dto);
}

// Application/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public ProductService(
        IRepository<Product> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        var entity = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        var created = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(created);
    }
}

// Controller
public async Task<IActionResult> Create(ProductDto dto)
{
    if (ModelState.IsValid)
    {
        await _productService.CreateAsync(dto);
        return RedirectToAction("Index");
    }
    return View(dto);
}
```

**Explanation**: Move business logic to service layer, keep controllers thin.

---

### Rule 12: Error Handling and Logging
**Legacy:**
```csharp
public ActionResult Index()
{
    var products = _context.Products.ToList();
    return View(products);
}
```

**Modern:**
```csharp
public async Task<IActionResult> Index()
{
    try
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving products");
        return View("Error");
    }
}
```

**Explanation**: Add proper error handling and logging throughout the application.

---

### Rule 13: Connection String Configuration
**Legacy:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-OrbitAOS.V6-afe03967;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Modern:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS.V6;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Explanation**: Add `TrustServerCertificate=True` for .NET 8 SQL Server connections.

---

### Rule 14: Nullable Reference Types
**Legacy:**
```csharp
public class Product
{
    public string Name { get; set; }
}
```

**Modern:**
```csharp
public class Product
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

**Explanation**: Enable nullable reference types and properly annotate properties.

---

### Rule 15: View Imports Update
**Legacy:**
```cshtml
@using OrbitAOS.V6
@using OrbitAOS.V6.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**Modern:**
```cshtml
@using OrbitAOS.V6.Web
@using OrbitAOS.V6.Web.Models
@using OrbitAOS.V6.Application.DTOs
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**Explanation**: Update view imports to reflect new namespace structure.

---

### Rule 16: Identity Configuration
**Legacy:**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Modern:**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
```

**Explanation**: Configure Identity options explicitly for better security.

---

### Rule 17: Middleware Pipeline Configuration
**Legacy:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

**Modern:**
```csharp
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
```

**Explanation**: Ensure correct middleware order and include all necessary middleware.

---

### Rule 18: Logging Configuration
**Legacy:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Modern:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "OrbitAOS.V6": "Debug"
    }
  }
}
```

**Explanation**: Configure logging levels for different namespaces.

---

### Rule 19: Project References
**Legacy:**
```xml
<!-- Single project, no references -->
```

**Modern:**
```xml
<!-- Web project -->
<ItemGroup>
  <ProjectReference Include="..\OrbitAOS.V6.Application\OrbitAOS.V6.Application.csproj" />
  <ProjectReference Include="..\OrbitAOS.V6.Infrastructure\OrbitAOS.V6.Infrastructure.csproj" />
</ItemGroup>

<!-- Infrastructure project -->
<ItemGroup>
  <ProjectReference Include="..\OrbitAOS.V6.Domain\OrbitAOS.V6.Domain.csproj" />
  <ProjectReference Include="..\OrbitAOS.V6.Application\OrbitAOS.V6.Application.csproj" />
</ItemGroup>

<!-- Application project -->
<ItemGroup>
  <ProjectReference Include="..\OrbitAOS.V6.Domain\OrbitAOS.V6.Domain.csproj" />
</ItemGroup>
```

**Explanation**: Set up proper project references following Clean Architecture dependencies.

---

### Rule 20: DTO Creation Pattern
**Legacy:**
```csharp
// Using entities directly in views
@model Product
```

**Modern:**
```csharp
// Application/DTOs/ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

// View
@model ProductDto
```

**Explanation**: Create DTOs for data transfer, keep validation attributes on DTOs.

---

### Rule 21: Service Registration Pattern
**Legacy:**
```csharp
// Program.cs
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
// ... many more
```

**Modern:**
```csharp
// Application/Common/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        // ... more services
        
        return services;
    }
}

// Program.cs
builder.Services.AddApplicationServices();
```

**Explanation**: Group service registrations by layer using extension methods.

---

### Rule 22: Entity Configuration Pattern
**Legacy:**
```csharp
public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
}
```

**Modern:**
```csharp
// Domain/Entities/Product.cs (no data annotations)
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Infrastructure/Data/Configurations/ProductConfiguration.cs
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)");
    }
}

// ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ApplyConfiguration(new ProductConfiguration());
}
```

**Explanation**: Move entity configuration to separate configuration classes.

---

### Rule 23: Async Controller Actions
**Legacy:**
```csharp
public ActionResult Index()
{
    var products = _context.Products.ToList();
    return View(products);
}

public ActionResult Details(int id)
{
    var product = _context.Products.Find(id);
    return View(product);
}
```

**Modern:**
```csharp
public async Task<IActionResult> Index()
{
    var products = await _productService.GetAllAsync();
    return View(products);
}

public async Task<IActionResult> Details(int id)
{
    var product = await _productService.GetByIdAsync(id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}
```

**Explanation**: All controller actions should be async for better performance.

---

### Rule 24: Transaction Management
**Legacy:**
```csharp
public ActionResult CreateOrder(Order order)
{
    _context.Orders.Add(order);
    _context.SaveChanges();
    
    foreach (var item in order.Items)
    {
        _context.OrderItems.Add(item);
    }
    _context.SaveChanges();
    
    return RedirectToAction("Index");
}
```

**Modern:**
```csharp
public async Task<IActionResult> CreateOrder(OrderDto dto)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        var order = await _orderService.CreateAsync(dto);
        
        foreach (var item in dto.Items)
        {
            await _orderItemService.CreateAsync(order.Id, item);
        }
        
        await _unitOfWork.CommitTransactionAsync();
        
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error creating order");
        return View("Error");
    }
}
```

**Explanation**: Use Unit of Work for transaction management.

---

### Rule 25: Validation Pattern
**Legacy:**
```csharp
public ActionResult Create(Product product)
{
    if (string.IsNullOrEmpty(product.Name))
    {
        ModelState.AddModelError("Name", "Name is required");
    }
    
    if (product.Price <= 0)
    {
        ModelState.AddModelError("Price", "Price must be greater than 0");
    }
    
    if (ModelState.IsValid)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    
    return View(product);
}
```

**Modern:**
```csharp
// DTO with validation attributes
public class ProductDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    public decimal Price { get; set; }
}

// Controller
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(ProductDto dto)
{
    if (!ModelState.IsValid)
    {
        return View(dto);
    }
    
    try
    {
        await _productService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating product");
        ModelState.AddModelError("", "An error occurred while creating the product");
        return View(dto);
    }
}
```

**Explanation**: Use data annotations on DTOs for validation.

---

### Rule 26: Query Optimization
**Legacy:**
```csharp
public ActionResult Index()
{
    var products = _context.Products
        .Include(p => p.Category)
        .Include(p => p.Supplier)
        .ToList();
    return View(products);
}
```

**Modern:**
```csharp
// Application/Interfaces/IProductService.cs
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllWithDetailsAsync();
}

// Application/Services/ProductService.cs
public async Task<IEnumerable<ProductDto>> GetAllWithDetailsAsync()
{
    var products = await _repository.GetAllAsync();
    // Map to DTOs with only required data
    return products.Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        CategoryName = p.Category?.Name,
        SupplierName = p.Supplier?.Name
    });
}

// Controller
public async Task<IActionResult> Index()
{
    var products = await _productService.GetAllWithDetailsAsync();
    return View(products);
}
```

**Explanation**: Optimize queries and return only required data through DTOs.

---

### Rule 27: Configuration Management
**Legacy:**
```csharp
var connectionString = Configuration.GetConnectionString("DefaultConnection");
var apiKey = Configuration["ApiKey"];
```

**Modern:**
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "AppSettings": {
    "ApiKey": "...",
    "MaxUploadSize": 10485760,
    "EnableFeatureX": true
  }
}

// Configuration class
public class AppSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public long MaxUploadSize { get; set; }
    public bool EnableFeatureX { get; set; }
}

// Program.cs
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Usage in service
public class SomeService
{
    private readonly AppSettings _settings;
    
    public SomeService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }
}
```

**Explanation**: Use strongly-typed configuration with IOptions pattern.

---

### Rule 28: Response Caching
**Legacy:**
```csharp
[OutputCache(Duration = 3600)]
public ActionResult Index()
{
    var products = _context.Products.ToList();
    return View(products);
}
```

**Modern:**
```csharp
// Program.cs
builder.Services.AddResponseCaching();

var app = builder.Build();
app.UseResponseCaching();

// Controller
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> Index()
{
    var products = await _productService.GetAllAsync();
    return View(products);
}
```

**Explanation**: Use ResponseCache attribute instead of OutputCache.

---

### Rule 29: Health Checks
**Legacy:**
```csharp
// No health checks
```

**Modern:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

app.MapHealthChecks("/health");

// Access at: https://localhost:5001/health
```

**Explanation**: Add health checks for monitoring application status.

---

### Rule 30: API Versioning (if applicable)
**Legacy:**
```csharp
[Route("api/products")]
public class ProductsController : ControllerBase
{
    // ...
}
```

**Modern:**
```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Controller
[ApiController]
[Route("api/v{version:apiVersion}/products")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    // ...
}
```

**Explanation**: Implement API versioning for better API management.

---

### Rule 31: Global Exception Handling
**Legacy:**
```csharp
// Exception handling in each controller
public ActionResult Index()
{
    try
    {
        var products = _context.Products.ToList();
        return View(products);
    }
    catch (Exception ex)
    {
        // Log and handle
        return View("Error");
    }
}
```

**Modern:**
```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        return context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An error occurred processing your request"
        });
    }
}

// Program.cs
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

**Explanation**: Implement global exception handling middleware.

---

### Rule 32: Pagination Pattern
**Legacy:**
```csharp
public ActionResult Index()
{
    var products = _context.Products.ToList();
    return View(products);
}
```

**Modern:**
```csharp
// Application/DTOs/PaginatedList.cs
public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}

// Service
public async Task<PaginatedList<ProductDto>> GetPaginatedAsync(
    int pageIndex, int pageSize)
{
    var totalCount = await _repository.CountAsync();
    var items = await _repository.GetPagedAsync(pageIndex, pageSize);
    
    return new PaginatedList<ProductDto>
    {
        Items = items.Select(MapToDto).ToList(),
        PageIndex = pageIndex,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        TotalCount = totalCount
    };
}

// Controller
public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
{
    var products = await _productService.GetPaginatedAsync(pageIndex, pageSize);
    return View(products);
}
```

**Explanation**: Implement pagination for large datasets.

---

## Data Strategy with EF Core

### Database-First Approach (Scaffold from Existing Database)

#### Step 1: Install EF Core Tools
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

#### Step 2: Scaffold DbContext and Entities
```bash
cd OrbitAOS.V6.Infrastructure

dotnet ef dbcontext scaffold \
    "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS.V6;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir Data/Entities \
    --context-dir Data \
    --context ApplicationDbContext \
    --force \
    --no-onconfiguring \
    --startup-project ../OrbitAOS.V6.Web
```

**Parameters Explained:**
- `Server=(localdb)\\mssqllocaldb`: SQL Server instance
- `Database=OrbitAOS.V6`: Database name
- `Trusted_Connection=True`: Use Windows authentication
- `MultipleActiveResultSets=true`: Allow multiple result sets
- `TrustServerCertificate=True`: Trust server certificate (.NET 8 requirement)
- `--output-dir Data/Entities`: Output directory for entity classes
- `--context-dir Data`: Output directory for DbContext
- `--context ApplicationDbContext`: DbContext class name
- `--force`: Overwrite existing files
- `--no-onconfiguring`: Don't generate OnConfiguring method
- `--startup-project`: Specify startup project for configuration

#### Step 3: Move Entities to Domain Layer
```bash
# After scaffolding, move entities to Domain layer
mv OrbitAOS.V6.Infrastructure/Data/Entities/* OrbitAOS.V6.Domain/Entities/
```

#### Step 4: Update DbContext
```csharp
// Update namespace references in ApplicationDbContext
using OrbitAOS.V6.Domain.Entities;
```

---

### Code-First Approach (Migrations)

#### Step 1: Create Initial Migration
```bash
cd OrbitAOS.V6.Web

dotnet ef migrations add InitialCreate \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project . \
    --context ApplicationDbContext \
    --output-dir Data/Migrations
```

**Parameters Explained:**
- `InitialCreate`: Migration name
- `--project`: Project containing DbContext
- `--startup-project`: Project with configuration
- `--context`: DbContext class name
- `--output-dir`: Output directory for migration files

#### Step 2: Review Migration
```csharp
// Check generated migration file
// OrbitAOS.V6.Infrastructure/Data/Migrations/YYYYMMDDHHMMSS_InitialCreate.cs

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Review table creation statements
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Review rollback statements
    }
}
```

#### Step 3: Apply Migration
```bash
dotnet ef database update \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project .
```

#### Step 4: Add Subsequent Migrations
```bash
# After making entity changes
dotnet ef migrations add AddProductCategory \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project .

# Apply migration
dotnet ef database update \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project .
```

#### Step 5: Remove Last Migration (if needed)
```bash
dotnet ef migrations remove \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project .
```

#### Step 6: Generate SQL Script
```bash
# Generate script for all migrations
dotnet ef migrations script \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project . \
    --output migration.sql

# Generate script for specific migration range
dotnet ef migrations script \
    --project ../OrbitAOS.V6.Infrastructure \
    --startup-project . \
    --from InitialCreate \
    --to AddProductCategory \
    --output migration_range.sql
```

---

### Hybrid Approach

#### Scenario: Existing database with new features

1. **Scaffold existing database**
   ```bash
   dotnet ef dbcontext scaffold "..." \
       Microsoft.EntityFrameworkCore.SqlServer \
       --output-dir Data/Entities \
       --force
   ```

2. **Add new entities manually**
   ```csharp
   // Domain/Entities/NewFeature.cs
   public class NewFeature : BaseEntity
   {
       public string Name { get; set; }
   }
   ```

3. **Create migration for new entities**
   ```bash
   dotnet ef migrations add AddNewFeature \
       --project ../OrbitAOS.V6.Infrastructure \
       --startup-project .
   ```

4. **Apply migration**
   ```bash
   dotnet ef database update \
       --project ../OrbitAOS.V6.Infrastructure \
       --startup-project .
   ```

---

### DbContext Configuration Best Practices

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Or apply individual configurations
        builder.ApplyConfiguration(new ProductConfiguration());
        builder.ApplyConfiguration(new CategoryConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
    }
}
```

---

### Connection String Management

#### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS.V6.Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=OrbitAOS.V6;User Id=app_user;Password=***;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=True"
  }
}
```

#### Using Environment Variables
```bash
# Set environment variable
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"

# Or in Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
```

---

### Repository Pattern with EF Core

```csharp
// Application/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(int id);
}

// Infrastructure/Data/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }
    
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }
    
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
    
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
    
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
    
    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}
```

---

## Authentication Migration

### Scenario 1: Forms Authentication → Cookie Authentication

#### Legacy (Forms Authentication in web.config)
```xml
<system.web>
  <authentication mode="Forms">
    <forms loginUrl="~/Account/Login" timeout="2880" />
  </authentication>
</system.web>
```

#### Modern (Cookie Authentication in Program.cs)
```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

#### Login Implementation
```csharp
// Controllers/AccountController.cs
public class AccountController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // Validate user credentials (your logic here)
        var user = await ValidateUserAsync(model.Username, model.Password);
        
        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(2)
            };
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            
            return RedirectToAction("Index", "Home");
        }
        
        ModelState.AddModelError("", "Invalid username or password");
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
```

---

### Scenario 2: ASP.NET Identity 2.x → ASP.NET Core Identity

#### Database Schema Compatibility
ASP.NET Core Identity uses a similar schema to ASP.NET Identity 2.x, but with some differences:

**Compatible Tables:**
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims

**Migration Steps:**

1. **Install Identity Packages**
   ```bash
   cd OrbitAOS.V6.Infrastructure
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
   
   cd ../OrbitAOS.V6.Web
   dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
   ```

2. **Update DbContext**
   ```csharp
   // Infrastructure/Data/ApplicationDbContext.cs
   using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
   
   public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
   {
       public ApplicationDbContext(
           DbContextOptions<ApplicationDbContext> options)
           : base(options)
       {
       }
       
       // Your DbSets...
       
       protected override void OnModelCreating(ModelBuilder builder)
       {
           base.OnModelCreating(builder);
           
           // Customize Identity table names if needed
           builder.Entity<ApplicationUser>(entity =>
           {
               entity.ToTable("AspNetUsers");
           });
           
           builder.Entity<IdentityRole>(entity =>
           {
               entity.ToTable("AspNetRoles");
           });
           
           // Other configurations...
       }
   }
   ```

3. **Create ApplicationUser**
   ```csharp
   // Domain/Entities/ApplicationUser.cs
   using Microsoft.AspNetCore.Identity;
   
   public class ApplicationUser : IdentityUser
   {
       public string FirstName { get; set; } = string.Empty;
       public string LastName { get; set; } = string.Empty;
       public DateTime? DateOfBirth { get; set; }
       public DateTime CreatedAt { get; set; }
   }
   ```

4. **Configure Identity in DI**
   ```csharp
   // Infrastructure/DependencyInjection.cs
   public static IServiceCollection AddInfrastructureServices(
       this IServiceCollection services,
       IConfiguration configuration)
   {
       // DbContext
       var connectionString = configuration
           .GetConnectionString("DefaultConnection");
       
       services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(connectionString));
       
       // Identity
       services.AddDefaultIdentity<ApplicationUser>(options =>
       {
           // Password settings
           options.Password.RequireDigit = true;
           options.Password.RequireLowercase = true;
           options.Password.RequireUppercase = true;
           options.Password.RequireNonAlphanumeric = true;
           options.Password.RequiredLength = 8;
           
           // Lockout settings
           options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
           options.Lockout.MaxFailedAccessAttempts = 5;
           options.Lockout.AllowedForNewUsers = true;
           
           // User settings
           options.User.RequireUniqueEmail = true;
           
           // Sign-in settings
           options.SignIn.RequireConfirmedEmail = true;
           options.SignIn.RequireConfirmedAccount = true;
       })
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<ApplicationDbContext>()
       .AddDefaultTokenProviders();
       
       // Cookie settings
       services.ConfigureApplicationCookie(options =>
       {
           options.LoginPath = "/Identity/Account/Login";
           options.LogoutPath = "/Identity/Account/Logout";
           options.AccessDeniedPath = "/Identity/Account/AccessDenied";
           options.ExpireTimeSpan = TimeSpan.FromDays(14);
           options.SlidingExpiration = true;
       });
       
       return services;
   }
   ```

5. **Scaffold Identity UI**
   ```bash
   cd OrbitAOS.V6.Web
   
   dotnet aspnet-codegenerator identity \
       --dbContext ApplicationDbContext \
       --files "Account.Register;Account.Login;Account.Logout" \
       --userClass ApplicationUser \
       --force
   ```

6. **Update Program.cs**
   ```csharp
   // Program.cs
   var app = builder.Build();
   
   // ...
   
   app.UseAuthentication();
   app.UseAuthorization();
   
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   app.MapRazorPages(); // Required for Identity UI
   
   app.Run();
   ```

7. **UserManager and SignInManager Usage**
   ```csharp
   // Controllers/AccountController.cs
   public class AccountController : Controller
   {
       private readonly UserManager<ApplicationUser> _userManager;
       private readonly SignInManager<ApplicationUser> _signInManager;
       private readonly ILogger<AccountController> _logger;
       
       public AccountController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           ILogger<AccountController> logger)
       {
           _userManager = userManager;
           _signInManager = signInManager;
           _logger = logger;
       }
       
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Register(RegisterViewModel model)
       {
           if (!ModelState.IsValid)
           {
               return View(model);
           }
           
           var user = new ApplicationUser
           {
               UserName = model.Email,
               Email = model.Email,
               FirstName = model.FirstName,
               LastName = model.LastName,
               CreatedAt = DateTime.UtcNow
           };
           
           var result = await _userManager.CreateAsync(user, model.Password);
           
           if (result.Succeeded)
           {
               _logger.LogInformation("User created a new account");
               
               // Send confirmation email
               var code = await _userManager
                   .GenerateEmailConfirmationTokenAsync(user);
               // Send email with code...
               
               await _signInManager.SignInAsync(user, isPersistent: false);
               return RedirectToAction("Index", "Home");
           }
           
           foreach (var error in result.Errors)
           {
               ModelState.AddModelError("", error.Description);
           }
           
           return View(model);
       }
       
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Login(LoginViewModel model)
       {
           if (!ModelState.IsValid)
           {
               return View(model);
           }
           
           var result = await _signInManager.PasswordSignInAsync(
               model.Email,
               model.Password,
               model.RememberMe,
               lockoutOnFailure: true);
           
           if (result.Succeeded)
           {
               _logger.LogInformation("User logged in");
               return RedirectToAction("Index", "Home");
           }
           
           if (result.RequiresTwoFactor)
           {
               return RedirectToAction("LoginWith2fa", new { model.RememberMe });
           }
           
           if (result.IsLockedOut)
           {
               _logger.LogWarning("User account locked out");
               return View("Lockout");
           }
           
           ModelState.AddModelError("", "Invalid login attempt");
           return View(model);
       }
       
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Logout()
       {
           await _signInManager.SignOutAsync();
           _logger.LogInformation("User logged out");
           return RedirectToAction("Index", "Home");
       }
   }
   ```

8. **Role-Based Authorization**
   ```csharp
   // Seed roles
   public static class IdentityDataSeeder
   {
       public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
       {
           string[] roles = { "Admin", "Manager", "User" };
           
           foreach (var role in roles)
           {
               if (!await roleManager.RoleExistsAsync(role))
               {
                   await roleManager.CreateAsync(new IdentityRole(role));
               }
           }
       }
       
       public static async Task SeedAdminUserAsync(
           UserManager<ApplicationUser> userManager)
       {
           var adminEmail = "admin@orbitaos.com";
           
           if (await userManager.FindByEmailAsync(adminEmail) == null)
           {
               var adminUser = new ApplicationUser
               {
                   UserName = adminEmail,
                   Email = adminEmail,
                   FirstName = "Admin",
                   LastName = "User",
                   EmailConfirmed = true,
                   CreatedAt = DateTime.UtcNow
               };
               
               var result = await userManager.CreateAsync(adminUser, "Admin@123");
               
               if (result.Succeeded)
               {
                   await userManager.AddToRoleAsync(adminUser, "Admin");
               }
           }
       }
   }
   
   // Program.cs
   using (var scope = app.Services.CreateScope())
   {
       var services = scope.ServiceProvider;
       var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
       var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
       
       await IdentityDataSeeder.SeedRolesAsync(roleManager);
       await IdentityDataSeeder.SeedAdminUserAsync(userManager);
   }
   
   // Controller with role authorization
   [Authorize(Roles = "Admin")]
   public class AdminController : Controller
   {
       // Only accessible by Admin role
   }
   
   [Authorize(Roles = "Admin,Manager")]
   public class ManagementController : Controller
   {
       // Accessible by Admin or Manager roles
   }
   ```

9. **Claims-Based Authorization**
   ```csharp
   // Add claims to user
   var user = await _userManager.FindByEmailAsync(email);
   await _userManager.AddClaimAsync(user, 
       new Claim("Department", "IT"));
   await _userManager.AddClaimAsync(user, 
       new Claim("EmployeeNumber", "12345"));
   
   // Policy-based authorization
   // Program.cs
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("ITDepartmentOnly", policy =>
           policy.RequireClaim("Department", "IT"));
       
       options.AddPolicy("SeniorEmployee", policy =>
           policy.RequireAssertion(context =>
               context.User.HasClaim(c => 
                   c.Type == "EmployeeNumber" && 
                   int.Parse(c.Value) < 10000)));
   });
   
   // Controller with policy authorization
   [Authorize(Policy = "ITDepartmentOnly")]
   public class ITController : Controller
   {
       // Only accessible by IT department
   }
   ```

---

## Blockers and Mitigations

### Blocker 1: System.Web Dependencies
**Description**: Legacy code uses System.Web namespace which doesn't exist in .NET Core/8.

**Impact**: HIGH

**Examples**:
```csharp
// Legacy
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

HttpContext.Current.Session["key"] = value;
var path = Server.MapPath("~/uploads");
```

**Mitigation**:
```csharp
// Modern
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Session
services.AddSession();
app.UseSession();
HttpContext.Session.SetString("key", value);

// Path mapping
private readonly IWebHostEnvironment _env;
var path = Path.Combine(_env.WebRootPath, "uploads");

// HttpContext access in services
private readonly IHttpContextAccessor _httpContextAccessor;
var context = _httpContextAccessor.HttpContext;
```

---

### Blocker 2: Global.asax Application Events
**Description**: Global.asax doesn't exist in ASP.NET Core.

**Impact**: HIGH

**Legacy**:
```csharp
// Global.asax.cs
public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }
    
    protected void Application_Error()
    {
        var exception = Server.GetLastError();
        // Log error
    }
}
```

**Mitigation**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Application_Start equivalent
builder.Services.AddControllersWithViews();
// Configure services...

var app = builder.Build();

// Application_Error equivalent
app.UseExceptionHandler("/Home/Error");

// Or custom middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log error
        throw;
    }
});

app.Run();
```

---

### Blocker 3: Session State Implementation
**Description**: Session state requires explicit configuration in ASP.NET Core.

**Impact**: MEDIUM

**Legacy**:
```csharp
// web.config
<sessionState mode="InProc" timeout="20" />

// Usage
Session["UserId"] = userId;
var userId = (int)Session["UserId"];
```

**Mitigation**:
```csharp
// Program.cs
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.UseSession();

// Usage
HttpContext.Session.SetInt32("UserId", userId);
var userId = HttpContext.Session.GetInt32("UserId");

// Or use extension methods
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }
    
    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

---

### Blocker 4: Synchronous I/O Operations
**Description**: ASP.NET Core prefers async operations for better scalability.

**Impact**: MEDIUM

**Legacy**:
```csharp
public ActionResult Index()
{
    var products = _context.Products.ToList();
    return View(products);
}
```

**Mitigation**:
```csharp
public async Task<IActionResult> Index()
{
    var products = await _productService.GetAllAsync();
    return View(products);
}

// Enable synchronous I/O if absolutely necessary (not recommended)
// Program.cs
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
```

---

### Blocker 5: web.config Configuration
**Description**: web.config is replaced by appsettings.json and Program.cs.

**Impact**: HIGH

**Legacy**:
```xml
<!-- web.config -->
<configuration>
  <appSettings>
    <add key="ApiKey" value="12345" />
  </appSettings>
  <connectionStrings>
    <add name="DefaultConnection" connectionString="..." />
  </connectionStrings>
  <system.web>
    <compilation debug="true" targetFramework="4.7.2" />
    <httpRuntime targetFramework="4.7.2" />
  </system.web>
</configuration>
```

**Mitigation**:
```json
// appsettings.json
{
  "AppSettings": {
    "ApiKey": "12345"
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

```csharp
// Access configuration
var apiKey = builder.Configuration["AppSettings:ApiKey"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Or use strongly-typed configuration
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
```

---

### Blocker 6: Custom HTTP Modules
**Description**: HTTP modules are replaced by middleware in ASP.NET Core.

**Impact**: MEDIUM

**Legacy**:
```csharp
// Custom HTTP Module
public class CustomModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        context.BeginRequest += OnBeginRequest;
    }
    
    private void OnBeginRequest(object sender, EventArgs e)
    {
        // Custom logic
    }
    
    public void Dispose() { }
}

// web.config
<system.webServer>
  <modules>
    <add name="CustomModule" type="MyApp.CustomModule" />
  </modules>
</system.webServer>
```

**Mitigation**:
```csharp
// Custom Middleware
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    
    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Before request processing
        
        await _next(context);
        
        // After request processing
    }
}

// Extension method
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}

// Program.cs
app.UseCustomMiddleware();
```

---

### Blocker 7: Custom HTTP Handlers
**Description**: HTTP handlers are replaced by endpoints in ASP.NET Core.

**Impact**: MEDIUM

**Legacy**:
```csharp
// Custom HTTP Handler
public class ImageHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        // Process image request
    }
    
    public bool IsReusable => false;
}

// web.config
<system.webServer>
  <handlers>
    <add name="ImageHandler" path="*.img" verb="*" type="MyApp.ImageHandler" />
  </handlers>
</system.webServer>
```

**Mitigation**:
```csharp
// Program.cs
app.MapGet("/images/{filename}", async (string filename, IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.WebRootPath, "images", filename);
    
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }
    
    var bytes = await File.ReadAllBytesAsync(path);
    return Results.File(bytes, "image/jpeg");
});

// Or create a controller
[Route("api/images")]
public class ImageController : ControllerBase
{
    [HttpGet("{filename}")]
    public async Task<IActionResult> GetImage(string filename)
    {
        // Process image request
    }
}
```

---

### Blocker 8: OutputCache Attribute
**Description**: OutputCache attribute is replaced by ResponseCache.

**Impact**: LOW

**Legacy**:
```csharp
[OutputCache(Duration = 3600, VaryByParam = "id")]
public ActionResult Details(int id)
{
    var product = _context.Products.Find(id);
    return View(product);
}
```

**Mitigation**:
```csharp
// Program.cs
builder.Services.AddResponseCaching();

var app = builder.Build();
app.UseResponseCaching();

// Controller
[ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "id" })]
public async Task<IActionResult> Details(int id)
{
    var product = await _productService.GetByIdAsync(id);
    return View(product);
}

// Or use output caching (new in .NET 7+)
builder.Services.AddOutputCache();

app.UseOutputCache();

[OutputCache(Duration = 3600)]
public async Task<IActionResult> Details(int id)
{
    var product = await _productService.GetByIdAsync(id);
    return View(product);
}
```

---

### Blocker 9: Child Actions (Html.Action, Html.RenderAction)
**Description**: Child actions are removed in ASP.NET Core.

**Impact**: MEDIUM

**Legacy**:
```csharp
// Controller
[ChildActionOnly]
public ActionResult UserMenu()
{
    var model = GetUserMenuData();
    return PartialView("_UserMenu", model);
}

// View
@Html.Action("UserMenu", "Account")
```

**Mitigation**:
```csharp
// Create View Component
public class UserMenuViewComponent : ViewComponent
{
    private readonly IUserService _userService;
    
    public UserMenuViewComponent(IUserService userService)
    {
        _userService = userService;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = await _userService.GetUserMenuDataAsync();
        return View(model);
    }
}

// View
@await Component.InvokeAsync("UserMenu")

// Or use Tag Helper
<vc:user-menu></vc:user-menu>
```

---

### Blocker 10: Custom Action Filters
**Description**: Action filter syntax has changed slightly.

**Impact**: LOW

**Legacy**:
```csharp
public class CustomActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Before action
        base.OnActionExecuting(filterContext);
    }
    
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        // After action
        base.OnActionExecuted(filterContext);
    }
}
```

**Mitigation**:
```csharp
// Synchronous filter
public class CustomActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Before action
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // After action
    }
}

// Asynchronous filter (preferred)
public class CustomAsyncActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Before action
        
        var resultContext = await next();
        
        // After action
    }
}

// Register globally
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CustomActionFilter>();
});

// Or use as attribute
[ServiceFilter(typeof(CustomActionFilter))]
public class ProductController : Controller
{
    // ...
}
```

---

### Blocker 11: .NET Framework-Only NuGet Packages
**Description**: Some NuGet packages don't support .NET 8.

**Impact**: HIGH

**Examples**:
- EntityFramework 6.x → Use EntityFrameworkCore 8.0
- System.Web.Optimization → Use LigerShark.WebOptimizer.Core
- Microsoft.AspNet.WebApi → Built into ASP.NET Core
- Newtonsoft.Json (if required) → Use System.Text.Json or add compatibility package

**Mitigation**:
```bash
# Remove old packages
dotnet remove package EntityFramework
dotnet remove package System.Web.Optimization

# Add new packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package LigerShark.WebOptimizer.Core --version 3.0.411

# If Newtonsoft.Json is required
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 8.0.0
```

```csharp
// Program.cs
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(); // If needed
```

---

### Blocker 12: Deployment Model Changes
**Description**: Deployment process differs from .NET Framework.

**Impact**: MEDIUM

**Legacy**:
- Deploy to IIS
- web.config for configuration
- .NET Framework runtime on server

**Mitigation**:
```bash
# Publish application
dotnet publish -c Release -o ./publish

# Self-contained deployment (includes runtime)
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Framework-dependent deployment (requires .NET 8 runtime on server)
dotnet publish -c Release -o ./publish
```

**IIS Configuration**:
1. Install .NET 8 Hosting Bundle
2. Create IIS site
3. Point to published folder
4. Configure application pool (No Managed Code)

**web.config for IIS**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\OrbitAOS.V6.Web.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

### Blocker 13: Missing TempData Provider
**Description**: TempData requires explicit configuration in some scenarios.

**Impact**: LOW

**Legacy**:
```csharp
// Works automatically
TempData["Message"] = "Success!";
```

**Mitigation**:
```csharp
// Program.cs (usually not needed, but if issues arise)
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

builder.Services.AddSession();

var app = builder.Build();
app.UseSession();
```

---

### Blocker 14: Custom Model Binders
**Description**: Model binder syntax has changed.

**Impact**: LOW

**Legacy**:
```csharp
public class CustomModelBinder : IModelBinder
{
    public object BindModel(ControllerContext controllerContext, 
        ModelBindingContext bindingContext)
    {
        // Binding logic
    }
}
```

**Mitigation**:
```csharp
public class CustomModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }
        
        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider
            .GetValue(modelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
        
        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        
        var value = valueProviderResult.FirstValue;
        
        // Custom binding logic
        var model = /* create model from value */;
        
        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }
}

// Register
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
});
```

---

### Blocker 15: Route Constraints Syntax
**Description**: Route constraint syntax has minor differences.

**Impact**: LOW

**Legacy**:
```csharp
routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
    constraints: new { id = @"\d+" }
);
```

**Mitigation**:
```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:int?}");

// Or attribute routing
[Route("products/{id:int}")]
public async Task<IActionResult> Details(int id)
{
    // ...
}

// Custom constraint
public class CustomRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext httpContext, IRouter route, 
        string routeKey, RouteValueDictionary values, 
        RouteDirection routeDirection)
    {
        // Custom logic
        return true;
    }
}

// Register
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("custom", typeof(CustomRouteConstraint));
});

// Use
[Route("products/{id:custom}")]
```

---

## Unit Testing Framework

### Test Project Setup

#### Create Test Projects
```bash
# Create test projects
dotnet new xunit -n OrbitAOS.V6.Domain.Tests
dotnet new xunit -n OrbitAOS.V6.Application.Tests
dotnet new xunit -n OrbitAOS.V6.Infrastructure.Tests
dotnet new xunit -n OrbitAOS.V6.Web.Tests

# Add to solution
dotnet sln add OrbitAOS.V6.Domain.Tests
dotnet sln add OrbitAOS.V6.Application.Tests
dotnet sln add OrbitAOS.V6.Infrastructure.Tests
dotnet sln add OrbitAOS.V6.Web.Tests

# Add project references
cd OrbitAOS.V6.Application.Tests
dotnet add reference ../OrbitAOS.V6.Application/OrbitAOS.V6.Application.csproj
dotnet add reference ../OrbitAOS.V6.Domain/OrbitAOS.V6.Domain.csproj

cd ../OrbitAOS.V6.Infrastructure.Tests
dotnet add reference ../OrbitAOS.V6.Infrastructure/OrbitAOS.V6.Infrastructure.csproj
dotnet add reference ../OrbitAOS.V6.Application/OrbitAOS.V6.Application.csproj
dotnet add reference ../OrbitAOS.V6.Domain/OrbitAOS.V6.Domain.csproj

cd ../OrbitAOS.V6.Web.Tests
dotnet add reference ../OrbitAOS.V6.Web/OrbitAOS.V6.Web.csproj
```

#### Install Testing Packages
```bash
# Moq for mocking
dotnet add package Moq --version 4.20.70

# FluentAssertions for better assertions
dotnet add package FluentAssertions --version 6.12.0

# For web tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0

# For EF Core in-memory database
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
```

---

### Unit Testing Examples

#### 1. Domain Entity Tests
```csharp
// OrbitAOS.V6.Domain.Tests/Entities/ProductTests.cs
using FluentAssertions;
using OrbitAOS.V6.Domain.Entities;
using Xunit;

namespace OrbitAOS.V6.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var product = new Product();
        
        // Assert
        product.Name.Should().BeEmpty();
        product.Price.Should().Be(0);
        product.IsActive.Should().BeFalse();
    }
    
    [Theory]
    [InlineData("Product 1", 10.99)]
    [InlineData("Product 2", 25.50)]
    public void Product_ShouldSetPropertiesCorrectly(string name, decimal price)
    {
        // Arrange
        var product = new Product
        {
            Name = name,
            Price = price,
            IsActive = true
        };
        
        // Assert
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.IsActive.Should().BeTrue();
    }
}
```

---

#### 2. Application Service Tests
```csharp
// OrbitAOS.V6.Application.Tests/Services/ProductServiceTests.cs
using FluentAssertions;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Application.Services;
using OrbitAOS.V6.Domain.Entities;
using Xunit;

namespace OrbitAOS.V6.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ProductService _service;
    
    public ProductServiceTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new ProductService(_mockRepository.Object, _mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99m },
            new Product { Id = 2, Name = "Product 2", Price = 25.50m }
        };
        
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);
        
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Product 1");
        result.Should().Contain(p => p.Name == "Product 2");
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product 1", Price = 10.99m };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        
        // Act
        var result = await _service.GetByIdAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Product 1");
        result.Price.Should().Be(10.99m);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);
        
        // Act
        var result = await _service.GetByIdAsync(999);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateProductAndReturnDto()
    {
        // Arrange
        var dto = new ProductDto
        {
            Name = "New Product",
            Price = 15.99m,
            IsActive = true
        };
        
        var createdProduct = new Product
        {
            Id = 1,
            Name = dto.Name,
            Price = dto.Price,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);
        
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        // Act
        var result = await _service.CreateAsync(dto);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(15.99m);
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateProduct()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Price = 10.99m,
            IsActive = true
        };
        
        var dto = new ProductDto
        {
            Id = 1,
            Name = "Updated Name",
            Price = 20.99m,
            IsActive = false
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);
        
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);
        
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        // Act
        await _service.UpdateAsync(1, dto);
        
        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Name == "Updated Name" &&
            p.Price == 20.99m &&
            p.IsActive == false
        )), Times.Once);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var dto = new ProductDto { Id = 999, Name = "Test" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, dto));
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);
        
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        // Act
        await _service.DeleteAsync(1);
        
        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

---

#### 3. Infrastructure Repository Tests
```csharp
// OrbitAOS.V6.Infrastructure.Tests/Data/RepositoryTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Infrastructure.Data;
using Xunit;

namespace OrbitAOS.V6.Infrastructure.Tests.Data;

public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Product> _repository;
    
    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new Repository<Product>(_context);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Name = "Product 1", Price = 10.99m },
            new Product { Name = "Product 2", Price = 25.50m }
        );
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetAllAsync();
        
        // Assert
        result.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Price = 10.99m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByIdAsync(product.Id);
        
        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
    }
    
    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var product = new Product { Name = "New Product", Price = 15.99m };
        
        // Act
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();
        
        // Assert
        var result = await _context.Products.FindAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Product");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var product = new Product { Name = "Original Name", Price = 10.99m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Act
        product.Name = "Updated Name";
        await _repository.UpdateAsync(product);
        await _context.SaveChangesAsync();
        
        // Assert
        var result = await _context.Products.FindAsync(product.Id);
        result!.Name.Should().Be("Updated Name");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var product = new Product { Name = "To Delete", Price = 10.99m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Act
        await _repository.DeleteAsync(product.Id);
        await _context.SaveChangesAsync();
        
        // Assert
        var result = await _context.Products.FindAsync(product.Id);
        result.Should().BeNull();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

#### 4. Controller Tests
```csharp
// OrbitAOS.V6.Web.Tests/Controllers/ProductControllerTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Web.Controllers;
using Xunit;

namespace OrbitAOS.V6.Web.Tests.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;
    
    public ProductControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mockService.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task Index_ShouldReturnViewWithProducts()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Product 1" },
            new ProductDto { Id = 2, Name = "Product 2" }
        };
        
        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);
        
        // Act
        var result = await _controller.Index();
        
        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        model.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task Details_WhenProductExists_ShouldReturnViewWithProduct()
    {
        // Arrange
        var product = new ProductDto { Id = 1, Name = "Product 1" };
        
        _mockService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(product);
        
        // Act
        var result = await _controller.Details(1);
        
        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductDto>().Subject;
        model.Name.Should().Be("Product 1");
    }
    
    [Fact]
    public async Task Details_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((ProductDto?)null);
        
        // Act
        var result = await _controller.Details(999);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [Fact]
    public async Task Create_Post_WithValidModel_ShouldRedirectToIndex()
    {
        // Arrange
        var dto = new ProductDto { Name = "New Product", Price = 10.99m };
        
        _mockService.Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(dto);
        
        // Act
        var result = await _controller.Create(dto);
        
        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }
    
    [Fact]
    public async Task Create_Post_WithInvalidModel_ShouldReturnViewWithModel()
    {
        // Arrange
        var dto = new ProductDto { Name = "New Product", Price = 10.99m };
        _controller.ModelState.AddModelError("Name", "Required");
        
        // Act
        var result = await _controller.Create(dto);
        
        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(dto);
    }
}
```

---

#### 5. Integration Tests
```csharp
// OrbitAOS.V6.Web.Tests/Integration/HomeControllerIntegrationTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OrbitAOS.V6.Web.Tests.Integration;

public class HomeControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public HomeControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Index")]
    [InlineData("/Home/Privacy")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync(url);
        
        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Contain("text/html");
    }
}
```

---

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test OrbitAOS.V6.Application.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProductServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ProductServiceTests.GetAllAsync_ShouldReturnAllProducts"
```

---

### Test Coverage

Install coverage tools:
```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate coverage report:
```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report
reportgenerator \
    -reports:"**/coverage.opencover.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:Html

# Open report
open coveragereport/index.html
```

---

## Execution Plan JSON

```json
{
  "migration_execution_plan": {
    "project_metadata": {
      "project_name": "OrbitAOS.V6",
      "source_framework": ".NET 6",
      "target_framework": ".NET 8",
      "architecture": "Clean Architecture (Domain/Application/Infrastructure/Web)",
      "migration_type": "Upgrade and Restructure",
      "estimated_duration": "2-3 weeks",
      "estimated_effort": "40-60 hours",
      "team_size": "2-3 developers",
      "complexity": "Medium"
    },
    "phases": [
      {
        "phase_number": 1,
        "phase_name": "Pre-Migration Assessment",
        "duration": "2 days",
        "tasks": [
          {
            "task_id": "1.1",
            "task_name": "Inventory Current Application",
            "description": "Document all controllers, views, models, and dependencies",
            "estimated_hours": 4,
            "deliverables": [
              "Application inventory document",
              "Component list",
              "Dependency matrix"
            ]
          },
          {
            "task_id": "1.2",
            "task_name": "Identify Dependencies",
            "description": "List all NuGet packages, third-party libraries, and custom components",
            "estimated_hours": 3,
            "deliverables": [
              "Dependency list",
              "Compatibility assessment"
            ]
          },
          {
            "task_id": "1.3",
            "task_name": "Risk Assessment",
            "description": "Identify potential blockers and breaking changes",
            "estimated_hours": 3,
            "deliverables": [
              "Risk assessment report",
              "Mitigation strategies"
            ]
          }
        ]
      },
      {
        "phase_number": 2,
        "phase_name": "Environment Setup",
        "duration": "2 days",
        "tasks": [
          {
            "task_id": "2.1",
            "task_name": "Install Required Tools",
            "description": "Install .NET 8 SDK, EF Core tools, and development tools",
            "estimated_hours": 2,
            "deliverables": [
              "Configured development environment"
            ]
          },
          {
            "task_id": "2.2",
            "task_name": "Create Solution Structure",
            "description": "Create Clean Architecture project structure",
            "estimated_hours": 4,
            "deliverables": [
              "Solution file",
              "Four project structure (Domain/Application/Infrastructure/Web)",
              "Project references configured"
            ]
          },
          {
            "task_id": "2.3",
            "task_name": "Install NuGet Packages",
            "description": "Install all required NuGet packages for .NET 8",
            "estimated_hours": 2,
            "deliverables": [
              "All packages installed",
              "Package compatibility verified"
            ]
          }
        ]
      },
      {
        "phase_number": 3,
        "phase_name": "Domain Layer Migration",
        "duration": "1 day",
        "tasks": [
          {
            "task_id": "3.1",
            "task_name": "Create Base Entity",
            "description": "Create base entity class with common properties",
            "estimated_hours": 1,
            "deliverables": [
              "BaseEntity.cs"
            ]
          },
          {
            "task_id": "3.2",
            "task_name": "Migrate Domain Entities",
            "description": "Move entities from old Models folder to Domain layer",
            "estimated_hours": 4,
            "deliverables": [
              "All domain entities migrated",
              "Entities inherit from BaseEntity"
            ]
          },
          {
            "task_id": "3.3",
            "task_name": "Create Domain Interfaces",
            "description": "Create domain service interfaces if needed",
            "estimated_hours": 2,
            "deliverables": [
              "Domain interfaces"
            ]
          }
        ]
      },
      {
        "phase_number": 4,
        "phase_name": "Application Layer Migration",
        "duration": "2 days",
        "tasks": [
          {
            "task_id": "4.1",
            "task_name": "Create Repository Interfaces",
            "description": "Create generic repository and specific repository interfaces",
            "estimated_hours": 3,
            "deliverables": [
              "IRepository<T>",
              "IUnitOfWork",
              "Specific repository interfaces"
            ]
          },
          {
            "task_id": "4.2",
            "task_name": "Create Service Interfaces",
            "description": "Create service interfaces for business logic",
            "estimated_hours": 4,
            "deliverables": [
              "Service interfaces for all business operations"
            ]
          },
          {
            "task_id": "4.3",
            "task_name": "Create DTOs",
            "description": "Create Data Transfer Objects for all entities",
            "estimated_hours": 3,
            "deliverables": [
              "DTOs for all entities"
            ]
          },
          {
            "task_id": "4.4",
            "task_name": "Implement Services",
            "description": "Implement business logic in service classes",
            "estimated_hours": 6,
            "deliverables": [
              "Service implementations",
              "Business logic migrated from controllers"
            ]
          },
          {
            "task_id": "4.5",
            "task_name": "Configure Dependency Injection",
            "description": "Set up DI for Application layer",
            "estimated_hours": 2,
            "deliverables": [
              "DependencyInjection.cs",
              "All services registered"
            ]
          }
        ]
      },
      {
        "phase_number": 5,
        "phase_name": "Infrastructure Layer Migration",
        "duration": "2 days",
        "tasks": [
          {
            "task_id": "5.1",
            "task_name": "Create DbContext",
            "description": "Create ApplicationDbContext with entity configurations",
            "estimated_hours": 4,
            "deliverables": [
              "ApplicationDbContext.cs",
              "Entity configurations"
            ]
          },
          {
            "task_id": "5.2",
            "task_name": "Implement Repository",
            "description": "Implement generic repository pattern",
            "estimated_hours": 4,
            "deliverables": [
              "Repository<T> implementation"
            ]
          },
          {
            "task_id": "5.3",
            "task_name": "Implement Unit of Work",
            "description": "Implement Unit of Work pattern for transaction management",
            "estimated_hours": 3,
            "deliverables": [
              "UnitOfWork implementation"
            ]
          },
          {
            "task_id": "5.4",
            "task_name": "Configure Infrastructure DI",
            "description": "Set up DI for Infrastructure layer",
            "estimated_hours": 2,
            "deliverables": [
              "Infrastructure DependencyInjection.cs",
              "DbContext and repositories registered"
            ]
          }
        ]
      },
      {
        "phase_number": 6,
        "phase_name": "Web Layer Migration",
        "duration": "1 day",
        "tasks": [
          {
            "task_id": "6.1",
            "task_name": "Update Program.cs",
            "description": "Configure middleware pipeline and services",
            "estimated_hours": 2,
            "deliverables": [
              "Program.cs configured",
              "All services registered"
            ]
          },
          {
            "task_id": "6.2",
            "task_name": "Migrate Controllers",
            "description": "Update controllers to use service layer",
            "estimated_hours": 6,
            "deliverables": [
              "All controllers migrated",
              "Controllers use service interfaces"
            ]
          },
          {
            "task_id": "6.3",
            "task_name": "Copy Views and Static Files",
            "description": "Copy views, wwwroot, and Areas",
            "estimated_hours": 2,
            "deliverables": [
              "All views copied",
              "Static files copied",
              "View imports updated"
            ]
          }
        ]
      },
      {
        "phase_number": 7,
        "phase_name": "Database Migration",
        "duration": "1 day",
        "tasks": [
          {
            "task_id": "7.1",
            "task_name": "Create Initial Migration",
            "description": "Generate EF Core migration for database schema",
            "estimated_hours": 2,
            "deliverables": [
              "Initial migration created"
            ]
          },
          {
            "task_id": "7.2",
            "task_name": "Review Migration",
            "description": "Review generated migration files",
            "estimated_hours": 1,
            "deliverables": [
              "Migration reviewed and approved"
            ]
          },
          {
            "task_id": "7.3",
            "task_name": "Update Database",
            "description": "Apply migration to database",
            "estimated_hours": 1,
            "deliverables": [
              "Database updated",
              "Schema verified"
            ]
          }
        ]
      },
      {
        "phase_number": 8,
        "phase_name": "Testing",
        "duration": "2 days",
        "tasks": [
          {
            "task_id": "8.1",
            "task_name": "Create Test Projects",
            "description": "Set up test projects for all layers",
            "estimated_hours": 2,
            "deliverables": [
              "Test projects created",
              "Testing packages installed"
            ]
          },
          {
            "task_id": "8.2",
            "task_name": "Write Unit Tests",
            "description": "Write unit tests for services and repositories",
            "estimated_hours": 8,
            "deliverables": [
              "Unit tests for all services",
              "Unit tests for repositories"
            ]
          },
          {
            "task_id": "8.3",
            "task_name": "Write Integration Tests",
            "description": "Write integration tests for controllers",
            "estimated_hours": 4,
            "deliverables": [
              "Integration tests for controllers"
            ]
          },
          {
            "task_id": "8.4",
            "task_name": "Run Tests",
            "description": "Execute all tests and verify coverage",
            "estimated_hours": 2,
            "deliverables": [
              "All tests passing",
              "Coverage report"
            ]
          }
        ]
      },
      {
        "phase_number": 9,
        "phase_name": "Build Verification",
        "duration": "1 day",
        "tasks": [
          {
            "task_id": "9.1",
            "task_name": "Clean Build",
            "description": "Perform clean build of entire solution",
            "estimated_hours": 1,
            "deliverables": [
              "Successful build"
            ]
          },
          {
            "task_id": "9.2",
            "task_name": "Run Application",
            "description": "Start application and verify it runs",
            "estimated_hours": 1,
            "deliverables": [
              "Application running"
            ]
          },
          {
            "task_id": "9.3",
            "task_name": "Manual Testing",
            "description": "Test all major features manually",
            "estimated_hours": 4,
            "deliverables": [
              "Manual testing completed",
              "Issues documented"
            ]
          },
          {
            "task_id": "9.4",
            "task_name": "Fix Issues",
            "description": "Fix any issues found during testing",
            "estimated_hours": 4,
            "deliverables": [
              "All issues resolved"
            ]
          }
        ]
      },
      {
        "phase_number": 10,
        "phase_name": "Documentation and Handover",
        "duration": "1 day",
        "tasks": [
          {
            "task_id": "10.1",
            "task_name": "Update Documentation",
            "description": "Update README and create architecture documentation",
            "estimated_hours": 3,
            "deliverables": [
              "README.md",
              "Architecture documentation",
              "Deployment guide"
            ]
          },
          {
            "task_id": "10.2",
            "task_name": "Create Migration Report",
            "description": "Document migration process and changes",
            "estimated_hours": 2,
            "deliverables": [
              "Migration report",
              "Known issues list"
            ]
          },
          {
            "task_id": "10.3",
            "task_name": "Team Training",
            "description": "Train team on new architecture and workflow",
            "estimated_hours": 3,
            "deliverables": [
              "Training session completed",
              "Team familiar with new structure"
            ]
          }
        ]
      }
    ],
    "folder_mapping": {
      "legacy_to_clean_architecture": [
        {
          "legacy": "/Models/ (Entities)",
          "modern": "/OrbitAOS.V6.Domain/Entities/",
          "notes": "Domain entities only, no data annotations"
        },
        {
          "legacy": "/Models/ (ViewModels)",
          "modern": "/OrbitAOS.V6.Web/Models/",
          "notes": "View-specific models"
        },
        {
          "legacy": "/Models/ (DTOs)",
          "modern": "/OrbitAOS.V6.Application/DTOs/",
          "notes": "Data transfer objects"
        },
        {
          "legacy": "/Data/ApplicationDbContext.cs",
          "modern": "/OrbitAOS.V6.Infrastructure/Data/ApplicationDbContext.cs",
          "notes": "DbContext with entity configurations"
        },
        {
          "legacy": "/Data/Migrations/",
          "modern": "/OrbitAOS.V6.Infrastructure/Data/Migrations/",
          "notes": "EF Core migrations"
        },
        {
          "legacy": "/Controllers/",
          "modern": "/OrbitAOS.V6.Web/Controllers/",
          "notes": "MVC controllers, now use service layer"
        },
        {
          "legacy": "/Views/",
          "modern": "/OrbitAOS.V6.Web/Views/",
          "notes": "Razor views"
        },
        {
          "legacy": "/wwwroot/",
          "modern": "/OrbitAOS.V6.Web/wwwroot/",
          "notes": "Static files"
        },
        {
          "legacy": "/Areas/",
          "modern": "/OrbitAOS.V6.Web/Areas/",
          "notes": "MVC areas"
        },
        {
          "legacy": "Program.cs",
          "modern": "/OrbitAOS.V6.Web/Program.cs",
          "notes": "Application entry point"
        },
        {
          "legacy": "appsettings.json",
          "modern": "/OrbitAOS.V6.Web/appsettings.json",
          "notes": "Configuration"
        },
        {
          "legacy": "N/A",
          "modern": "/OrbitAOS.V6.Application/Interfaces/",
          "notes": "Service interfaces (new)"
        },
        {
          "legacy": "N/A",
          "modern": "/OrbitAOS.V6.Application/Services/",
          "notes": "Business logic (new)"
        },
        {
          "legacy": "N/A",
          "modern": "/OrbitAOS.V6.Infrastructure/Data/Repository.cs",
          "notes": "Repository pattern (new)"
        },
        {
          "legacy": "N/A",
          "modern": "/OrbitAOS.V6.Infrastructure/Data/UnitOfWork.cs",
          "notes": "Unit of Work (new)"
        }
      ]
    },
    "transformation_rules_summary": {
      "total_rules": 32,
      "categories": [
        {
          "category": "Framework Updates",
          "rule_count": 3,
          "rules": [
            "Target framework update to net8.0",
            "Package version updates to 8.0.0",
            "Connection string TrustServerCertificate"
          ]
        },
        {
          "category": "Architecture Changes",
          "rule_count": 8,
          "rules": [
            "Controller direct DbContext → Service layer",
            "Synchronous → Asynchronous operations",
            "Entity → DTO mapping",
            "Domain entity structure with BaseEntity",
            "Repository pattern implementation",
            "Unit of Work pattern",
            "Service layer implementation",
            "Dependency injection by layer"
          ]
        },
        {
          "category": "Configuration",
          "rule_count": 5,
          "rules": [
            "DbContext configuration with Fluent API",
            "Identity configuration",
            "Middleware pipeline configuration",
            "Logging configuration",
            "Configuration management with IOptions"
          ]
        },
        {
          "category": "Best Practices",
          "rule_count": 8,
          "rules": [
            "Error handling and logging",
            "Nullable reference types",
            "View imports update",
            "Validation pattern with data annotations",
            "Query optimization",
            "Response caching",
            "Health checks",
            "Global exception handling"
          ]
        },
        {
          "category": "Advanced Patterns",
          "rule_count": 8,
          "rules": [
            "Entity configuration pattern",
            "Async controller actions",
            "Transaction management",
            "Pagination pattern",
            "Custom action filters",
            "Custom model binders",
            "Route constraints",
            "API versioning"
          ]
        }
      ]
    },
    "data_strategy": {
      "approach": "Code-First with Migrations",
      "database": "SQL Server",
      "orm": "Entity Framework Core 8.0",
      "key_commands": [
        {
          "command": "dotnet ef migrations add InitialCreate",
          "purpose": "Create initial migration",
          "parameters": "--project ../OrbitAOS.V6.Infrastructure --startup-project ."
        },
        {
          "command": "dotnet ef database update",
          "purpose": "Apply migrations to database",
          "parameters": "--project ../OrbitAOS.V6.Infrastructure --startup-project ."
        },
        {
          "command": "dotnet ef migrations remove",
          "purpose": "Remove last migration",
          "parameters": "--project ../OrbitAOS.V6.Infrastructure --startup-project ."
        },
        {
          "command": "dotnet ef migrations script",
          "purpose": "Generate SQL script",
          "parameters": "--project ../OrbitAOS.V6.Infrastructure --startup-project . --output migration.sql"
        }
      ]
    },
    "authentication_strategy": {
      "approach": "ASP.NET Core Identity",
      "user_type": "ApplicationUser (extends IdentityUser)",
      "features": [
        "User registration",
        "Login/Logout",
        "Password requirements",
        "Email confirmation",
        "Account lockout",
        "Role-based authorization",
        "Claims-based authorization"
      ]
    },
    "blockers": {
      "total_blockers": 15,
      "high_severity": 5,
      "medium_severity": 7,
      "low_severity": 3,
      "categories": [
        "System.Web dependencies",
        "Global.asax application events",
        "Session state implementation",
        "Synchronous I/O operations",
        "web.config configuration",
        "Custom HTTP modules",
        "Custom HTTP handlers",
        "OutputCache attribute",
        "Child actions",
        "Custom action filters",
        ".NET Framework-only packages",
        "Deployment model changes",
        "Missing TempData provider",
        "Custom model binders",
        "Route constraints syntax"
      ]
    },
    "testing_strategy": {
      "framework": "xUnit",
      "mocking": "Moq",
      "assertions": "FluentAssertions",
      "coverage_target": "80%",
      "test_types": [
        "Unit tests (Domain, Application, Infrastructure)",
        "Integration tests (Web)",
        "Controller tests",
        "Service tests",
        "Repository tests"
      ]
    },
    "tools_and_packages": {
      "required_tools": [
        {
          "tool": ".NET 8 SDK",
          "version": "8.0.0+",
          "purpose": "Development and build"
        },
        {
          "tool": "dotnet-ef",
          "version": "8.0.0",
          "purpose": "EF Core migrations"
        },
        {
          "tool": "Visual Studio 2022 or VS Code",
          "version": "Latest",
          "purpose": "IDE"
        }
      ],
      "nuget_packages": [
        {
          "package": "Microsoft.EntityFrameworkCore.SqlServer",
          "version": "8.0.0",
          "layer": "Infrastructure"
        },
        {
          "package": "Microsoft.EntityFrameworkCore.Tools",
          "version": "8.0.0",
          "layer": "Infrastructure"
        },
        {
          "package": "Microsoft.AspNetCore.Identity.EntityFrameworkCore",
          "version": "8.0.0",
          "layer": "Infrastructure"
        },
        {
          "package": "Microsoft.AspNetCore.Identity.UI",
          "version": "8.0.0",
          "layer": "Web"
        },
        {
          "package": "Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore",
          "version": "8.0.0",
          "layer": "Web"
        }
      ]
    },
    "success_criteria": {
      "functional": [
        "All routes and features work identically",
        "No missing functionality",
        "UI/UX preserved",
        "Authentication working"
      ],
      "technical": [
        "Solution builds successfully",
        "All tests passing",
        "Code coverage > 80%",
        "No compilation warnings"
      ],
      "performance": [
        "Response times within 10% of baseline",
        "Memory usage comparable or better"
      ],
      "quality": [
        "Clean Architecture implemented",
        "SOLID principles followed",
        "Code is maintainable and testable"
      ]
    },
    "rollback_plan": {
      "backup_strategy": "Git branch for legacy code",
      "rollback_steps": [
        "Stop new application",
        "Restore database backup",
        "Deploy legacy application",
        "Verify functionality"
      ]
    },
    "risk_assessment": {
      "high_risks": [
        {
          "risk": "Data loss during migration",
          "mitigation": "Full database backup before migration",
          "probability": "Low",
          "impact": "Critical"
        },
        {
          "risk": "Breaking changes in .NET 8",
          "mitigation": "Thorough testing and gradual rollout",
          "probability": "Medium",
          "impact": "High"
        }
      ],
      "medium_risks": [
        {
          "risk": "Performance degradation",
          "mitigation": "Performance testing and optimization",
          "probability": "Low",
          "impact": "Medium"
        },
        {
          "risk": "Third-party package incompatibility",
          "mitigation": "Research alternatives before migration",
          "probability": "Medium",
          "impact": "Medium"
        }
      ]
    },
    "timeline": {
      "total_duration": "2-3 weeks",
      "total_effort": "40-60 hours",
      "phases_breakdown": [
        {"phase": "Pre-Migration Assessment", "duration": "2 days", "effort": "10 hours"},
        {"phase": "Environment Setup", "duration": "2 days", "effort": "8 hours"},
        {"phase": "Domain Layer Migration", "duration": "1 day", "effort": "7 hours"},
        {"phase": "Application Layer Migration", "duration": "2 days", "effort": "18 hours"},
        {"phase": "Infrastructure Layer Migration", "duration": "2 days", "effort": "13 hours"},
        {"phase": "Web Layer Migration", "duration": "1 day", "effort": "10 hours"},
        {"phase": "Database Migration", "duration": "1 day", "effort": "4 hours"},
        {"phase": "Testing", "duration": "2 days", "effort": "16 hours"},
        {"phase": "Build Verification", "duration": "1 day", "effort": "10 hours"},
        {"phase": "Documentation and Handover", "duration": "1 day", "effort": "8 hours"}
      ]
    }
  }
}
```

---

## Conclusion

This comprehensive migration guide provides all the necessary information, code examples, and step-by-step instructions to successfully migrate the OrbitAOS.V6 application from ASP.NET MVC on .NET 6 to ASP.NET Core MVC on .NET 8 with Clean Architecture implementation.

### Key Achievements:
✅ Complete Clean Architecture structure
✅ Upgraded to .NET 8
✅ Repository Pattern and Unit of Work
✅ Comprehensive testing framework
✅ Detailed transformation rules
✅ Blocker mitigations
✅ Execution plan

### Next Steps:
1. Review this guide thoroughly
2. Follow the phase-by-phase migration plan
3. Execute transformations systematically
4. Test at each phase
5. Document any deviations or issues
6. Train team on new architecture

For questions or issues during migration, refer to the specific sections of this guide or consult the official Microsoft documentation.

---

**Document Version**: 1.0
**Last Updated**: 2025-01-07
**Author**: Migration Team
**Status**: Complete
