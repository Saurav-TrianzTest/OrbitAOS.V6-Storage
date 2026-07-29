# OrbitAOS.V6 - Clean Architecture Migration

## Overview
This project has been migrated from ASP.NET MVC on .NET 6 to ASP.NET Core MVC on .NET 8 using Clean Architecture principles.

## Architecture

The solution follows Clean Architecture with four distinct layers:

### 1. **Domain Layer** (`OrbitAOS.V6.Domain`)
- **Purpose**: Contains enterprise business rules and entities
- **Dependencies**: None (pure domain logic)
- **Contents**:
  - `Entities/`: Domain entities
  - `Common/`: Base classes and shared domain logic

### 2. **Application Layer** (`OrbitAOS.V6.Application`)
- **Purpose**: Contains application business rules and use cases
- **Dependencies**: Domain layer only
- **Contents**:
  - `Interfaces/`: Service and repository interfaces
  - `Services/`: Business logic implementation
  - `DTOs/`: Data Transfer Objects
  - `Common/`: Dependency injection configuration

### 3. **Infrastructure Layer** (`OrbitAOS.V6.Infrastructure`)
- **Purpose**: Contains external concerns (database, identity, external services)
- **Dependencies**: Domain and Application layers
- **Contents**:
  - `Data/`: DbContext, repositories, Unit of Work
  - `Identity/`: Identity configuration
  - `Services/`: Infrastructure service implementations

### 4. **Web Layer** (`OrbitAOS.V6.Web`)
- **Purpose**: Presentation layer (MVC controllers and views)
- **Dependencies**: Application and Infrastructure layers
- **Contents**:
  - `Controllers/`: MVC controllers
  - `Views/`: Razor views
  - `Models/`: View models
  - `wwwroot/`: Static files (CSS, JS, images)

## Key Features

### ✅ Upgraded to .NET 8
- Target framework: `net8.0`
- Latest package versions (8.0.0)
- Modern C# features enabled

### ✅ Clean Architecture Implementation
- Clear separation of concerns
- Dependency inversion principle
- Testable and maintainable code structure

### ✅ Repository Pattern & Unit of Work
- Generic repository for data access
- Unit of Work for transaction management
- Abstraction over Entity Framework Core

### ✅ Dependency Injection
- Configured in each layer
- Easy to extend and maintain
- Follows SOLID principles

### ✅ ASP.NET Core Identity
- Integrated authentication and authorization
- User management
- Role-based access control

## Project Structure

```
OrbitAOS.V6/
├── OrbitAOS.V6.Domain/
│   ├── Common/
│   │   └── BaseEntity.cs
│   ├── Entities/
│   │   └── SampleEntity.cs
│   └── OrbitAOS.V6.Domain.csproj
│
├── OrbitAOS.V6.Application/
│   ├── Common/
│   │   └── DependencyInjection.cs
│   ├── DTOs/
│   │   └── SampleDto.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── ISampleService.cs
│   ├── Services/
│   │   └── SampleService.cs
│   └── OrbitAOS.V6.Application.csproj
│
├── OrbitAOS.V6.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Repository.cs
│   │   └── UnitOfWork.cs
│   ├── DependencyInjection.cs
│   └── OrbitAOS.V6.Infrastructure.csproj
│
├── OrbitAOS.V6.Web/
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   └── SampleController.cs
│   ├── Views/
│   │   ├── Home/
│   │   ├── Shared/
│   │   └── _ViewImports.cshtml
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── lib/
│   ├── Models/
│   │   └── ErrorViewModel.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── OrbitAOS.V6.Web.csproj
│
└── OrbitAOS.V6.sln
```

## Getting Started

### Prerequisites
- .NET 8 SDK (8.0.0 or later)
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone or navigate to the project directory**
   ```bash
   cd /modernize-data/studio-data/TNT1001/APP508037/transformed-code/125/studio-workspace/DTNUPGRADE
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore OrbitAOS.V6.sln
   ```

3. **Update database connection string**
   Edit `OrbitAOS.V6.Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS.V6;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   cd OrbitAOS.V6.Web
   dotnet ef database update
   ```

5. **Build the solution**
   ```bash
   cd ..
   dotnet build OrbitAOS.V6.sln
   ```

6. **Run the application**
   ```bash
   cd OrbitAOS.V6.Web
   dotnet run
   ```

7. **Access the application**
   - Open browser: `https://localhost:5001` or `http://localhost:5000`

## Database Migrations

### Create a new migration
```bash
cd OrbitAOS.V6.Web
dotnet ef migrations add MigrationName --project ../OrbitAOS.V6.Infrastructure
```

### Update database
```bash
dotnet ef database update
```

### Remove last migration
```bash
dotnet ef migrations remove --project ../OrbitAOS.V6.Infrastructure
```

### Generate SQL script
```bash
dotnet ef migrations script --project ../OrbitAOS.V6.Infrastructure
```

## Adding New Features

### 1. Add a new entity (Domain Layer)
```csharp
// OrbitAOS.V6.Domain/Entities/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 2. Add DTO (Application Layer)
```csharp
// OrbitAOS.V6.Application/DTOs/ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 3. Add service interface and implementation
```csharp
// OrbitAOS.V6.Application/Interfaces/IProductService.cs
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
}

// OrbitAOS.V6.Application/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    
    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
    
    // Implementation...
}
```

### 4. Register service in DI
```csharp
// OrbitAOS.V6.Application/Common/DependencyInjection.cs
services.AddScoped<IProductService, ProductService>();
```

### 5. Add DbSet to DbContext
```csharp
// OrbitAOS.V6.Infrastructure/Data/ApplicationDbContext.cs
public DbSet<Product> Products { get; set; }
```

### 6. Create controller
```csharp
// OrbitAOS.V6.Web/Controllers/ProductController.cs
public class ProductController : Controller
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    // Actions...
}
```

## Testing

### Unit Tests
Create test projects for each layer:
```bash
dotnet new xunit -n OrbitAOS.V6.Domain.Tests
dotnet new xunit -n OrbitAOS.V6.Application.Tests
dotnet new xunit -n OrbitAOS.V6.Infrastructure.Tests
dotnet new xunit -n OrbitAOS.V6.Web.Tests
```

### Integration Tests
Use `WebApplicationFactory` for integration testing:
```csharp
public class HomeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public HomeControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }
}
```

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment-specific settings
- `appsettings.Development.json` - Development environment
- `appsettings.Production.json` - Production environment
- `appsettings.Staging.json` - Staging environment

## Security

### Identity Configuration
Located in `OrbitAOS.V6.Infrastructure/DependencyInjection.cs`:
- Password requirements
- Account confirmation
- Lockout settings

### Authentication & Authorization
- Cookie-based authentication
- Role-based authorization
- Claims-based authorization

## Performance Considerations

### Async/Await
All data access operations use async/await for better scalability.

### Caching
Consider adding caching for frequently accessed data:
```csharp
services.AddMemoryCache();
services.AddDistributedMemoryCache();
```

### Response Compression
Add response compression for better performance:
```csharp
services.AddResponseCompression();
```

## Deployment

### Publish the application
```bash
dotnet publish OrbitAOS.V6.Web/OrbitAOS.V6.Web.csproj -c Release -o ./publish
```

### Docker Support
Create a `Dockerfile` in the Web project:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrbitAOS.V6.Web/OrbitAOS.V6.Web.csproj", "OrbitAOS.V6.Web/"]
RUN dotnet restore "OrbitAOS.V6.Web/OrbitAOS.V6.Web.csproj"
COPY . .
WORKDIR "/src/OrbitAOS.V6.Web"
RUN dotnet build "OrbitAOS.V6.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrbitAOS.V6.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrbitAOS.V6.Web.dll"]
```

## Migration from .NET 6 to .NET 8

### Key Changes Made

1. **Target Framework**: Updated from `net6.0` to `net8.0`
2. **Package Versions**: All packages upgraded to 8.0.0
3. **Architecture**: Restructured into Clean Architecture layers
4. **Dependency Injection**: Organized by layer with extension methods
5. **Repository Pattern**: Implemented generic repository and Unit of Work
6. **Service Layer**: Business logic separated from controllers

### Breaking Changes
- Project structure completely reorganized
- Namespaces changed to reflect new architecture
- Controllers now depend on service interfaces instead of direct DbContext access

## Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Database Connection Issues
- Verify connection string in `appsettings.json`
- Ensure SQL Server is running
- Check firewall settings

### Migration Issues
```bash
# Drop database and recreate
dotnet ef database drop
dotnet ef database update
```

## Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## License
[Your License Here]

## Contributors
[Your Team/Contributors Here]

## Support
For issues and questions, please contact [Your Contact Information]
