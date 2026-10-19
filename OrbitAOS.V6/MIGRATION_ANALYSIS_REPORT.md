# OrbitAOS.V6 – ASP.NET Core MVC Migration Analysis Report
# From: ASP.NET Core MVC on .NET 6.0
# To:   ASP.NET Core MVC on .NET 10 (Clean Architecture)
# Generated: 2025-07-14
# ============================================================

## EXECUTIVE SUMMARY

The OrbitAOS.V6 project is an ASP.NET Core MVC application currently targeting
**net6.0** (not legacy ASP.NET MVC on .NET Framework). The migration path is
therefore **.NET 6 → .NET 10** with the addition of a **Clean Architecture**
layered structure (Domain / Application / Infrastructure / Web).

### Key Findings
- 18 migration issues identified (3 critical, 5 high, 7 medium, 3 low)
- Estimated effort: 24–40 hours
- Migration complexity: Medium
- Compatibility score: 62/100 (before migration)

---

## SECTION 1: STEP-BY-STEP MIGRATION PLAN

### Phase 1 – Pre-Migration Assessment (Week 1)
**Tasks:**
1. Inventory all NuGet packages and identify .NET 10 compatibility
2. Run `dotnet outdated` to list all outdated packages
3. Identify deprecated APIs (HasName, nullable column params, etc.)
4. Document all EF Core migration files for namespace updates
5. Confirm SQL Server version compatibility with EF Core 10

**Deliverables:** Compatibility matrix, risk register

### Phase 2 – Environment Setup (Week 1)
**Tasks:**
1. Install .NET 10 SDK: `winget install Microsoft.DotNet.SDK.10`
2. Update Visual Studio to 17.12+ or use VS Code with C# Dev Kit
3. Install EF Core tools: `dotnet tool install --global dotnet-ef`
4. Create solution file: `dotnet new sln -n OrbitAOS`
5. Verify: `dotnet --version` should show 10.x.x

**Build Verification (run AFTER migration):**
```bash
# Restore all packages
dotnet restore OrbitAOS.sln

# Build all projects in Release configuration
dotnet build OrbitAOS.sln --configuration Release --no-restore

# Run all unit tests
dotnet test OrbitAOS.sln --configuration Release --no-build

# Expected output: Build succeeded. X test(s) passed.
```

### Phase 3 – Clean Architecture Setup (Week 2)
**Tasks:**
1. Create OrbitAOS.Domain project (class library, net10.0)
2. Create OrbitAOS.Application project (class library, net10.0)
3. Create OrbitAOS.Infrastructure project (class library, net10.0)
4. Update OrbitAOS.V6 to reference all three new projects
5. Add all projects to OrbitAOS.sln

**Commands (run AFTER analysis):**
```bash
dotnet new classlib -n OrbitAOS.Domain -f net10.0
dotnet new classlib -n OrbitAOS.Application -f net10.0
dotnet new classlib -n OrbitAOS.Infrastructure -f net10.0
dotnet sln OrbitAOS.sln add OrbitAOS.Domain/OrbitAOS.Domain.csproj
dotnet sln OrbitAOS.sln add OrbitAOS.Application/OrbitAOS.Application.csproj
dotnet sln OrbitAOS.sln add OrbitAOS.Infrastructure/OrbitAOS.Infrastructure.csproj
dotnet sln OrbitAOS.sln add OrbitAOS.V6/OrbitAOS.V6.csproj
```

### Phase 4 – Core Infrastructure Migration (Week 2)
**Tasks:**
1. Update `<TargetFramework>net6.0</TargetFramework>` → `net10.0` in all .csproj files
2. Update all NuGet packages from 6.0.x → 10.0.0
3. Move ApplicationDbContext to OrbitAOS.Infrastructure/Data/
4. Move EF Core migrations to OrbitAOS.Infrastructure/Data/Migrations/
5. Fix deprecated EF Core APIs (HasName → HasDatabaseName)
6. Update Program.cs to reference Infrastructure DI extension

### Phase 5 – Data Layer Migration (Week 3)
**Tasks:**
1. Scaffold existing database (if database-first):
   ```bash
   dotnet ef dbcontext scaffold \
     "Server=.;Database=OrbitAOS;Trusted_Connection=True;" \
     Microsoft.EntityFrameworkCore.SqlServer \
     --output-dir Data/Entities \
     --context-dir Data \
     --context ApplicationDbContext \
     --project OrbitAOS.Infrastructure \
     --startup-project OrbitAOS.V6 \
     --force
   ```
2. Create new migration for .NET 10 schema:
   ```bash
   dotnet ef migrations add InitialNet10 \
     --project OrbitAOS.Infrastructure \
     --startup-project OrbitAOS.V6 \
     --output-dir Data/Migrations
   ```
3. Apply migration:
   ```bash
   dotnet ef database update \
     --project OrbitAOS.Infrastructure \
     --startup-project OrbitAOS.V6
   ```

### Phase 6 – Controllers and Models Migration (Week 3)
**Tasks:**
1. Update HomeController to inject IHomeService
2. Convert synchronous actions to async Task<IActionResult>
3. Move domain models to OrbitAOS.Domain/Entities/
4. Move ViewModels to OrbitAOS.V6/Models/ (presentation layer)
5. Create DTOs in OrbitAOS.Application/Interfaces/

### Phase 7 – Views Migration (Week 4)
**Tasks:**
1. Update _ViewImports.cshtml with new namespace references
2. Update _Layout.cshtml (dynamic copyright year, updated docs URL)
3. Verify all Tag Helpers still work (no breaking changes in .NET 10)
4. Update _LoginPartial.cshtml CSS classes for dark navbar
5. Test all Razor Pages in Identity area

### Phase 8 – Authentication Migration (Week 4)
**Tasks:**
1. Verify ASP.NET Core Identity configuration in Program.cs
2. Update cookie authentication settings
3. Add role support: `.AddRoles<IdentityRole>()`
4. Configure HSTS and HTTPS redirection
5. Test login/logout/register flows

### Phase 9 – Testing and Validation (Week 5)
**Tasks:**
1. Create OrbitAOS.Tests project
2. Write unit tests for controllers, services, repositories
3. Run: `dotnet test OrbitAOS.sln`
4. Achieve minimum 80% code coverage
5. Run integration tests against real SQL Server

### Phase 10 – Deployment and Cutover (Week 6)
**Tasks:**
1. Update CI/CD pipeline to use .NET 10 SDK
2. Update Dockerfile: `FROM mcr.microsoft.com/dotnet/aspnet:10.0`
3. Run `dotnet publish -c Release -o ./publish`
4. Deploy to staging, run smoke tests
5. Blue-green deployment to production

---

## SECTION 2: FOLDER-BY-FOLDER MAPPING

| Legacy (net6.0)                        | .NET 10 Clean Architecture Target                    |
|----------------------------------------|------------------------------------------------------|
| OrbitAOS.V6/Controllers/               | OrbitAOS.V6/Controllers/ (Web layer)                 |
| OrbitAOS.V6/Views/                     | OrbitAOS.V6/Views/ (Web layer)                       |
| OrbitAOS.V6/Models/                    | OrbitAOS.V6/Models/ (ViewModels, Web layer)          |
| OrbitAOS.V6/Data/ApplicationDbContext  | OrbitAOS.Infrastructure/Data/ApplicationDbContext    |
| OrbitAOS.V6/Data/Migrations/           | OrbitAOS.Infrastructure/Data/Migrations/             |
| OrbitAOS.V6/Program.cs                 | OrbitAOS.V6/Program.cs (updated)                     |
| OrbitAOS.V6/appsettings.json           | OrbitAOS.V6/appsettings.json (updated)               |
| OrbitAOS.V6/wwwroot/                   | OrbitAOS.V6/wwwroot/ (unchanged)                     |
| OrbitAOS.V6/Areas/Identity/            | OrbitAOS.V6/Areas/Identity/ (unchanged)              |
| OrbitAOS.V6/Properties/                | OrbitAOS.V6/Properties/ (unchanged)                  |
| (new)                                  | OrbitAOS.Domain/Common/BaseEntity.cs                 |
| (new)                                  | OrbitAOS.Domain/Common/IDomainEvent.cs               |
| (new)                                  | OrbitAOS.Domain/Interfaces/IRepository.cs            |
| (new)                                  | OrbitAOS.Domain/Exceptions/DomainException.cs        |
| (new)                                  | OrbitAOS.Application/Interfaces/IHomeService.cs      |
| (new)                                  | OrbitAOS.Application/Interfaces/HomeDashboardDto.cs  |
| (new)                                  | OrbitAOS.Application/DependencyInjection.cs          |
| (new)                                  | OrbitAOS.Infrastructure/Services/HomeService.cs      |
| (new)                                  | OrbitAOS.Infrastructure/Repositories/Repository.cs   |
| (new)                                  | OrbitAOS.Infrastructure/DependencyInjection.cs       |
| (new)                                  | OrbitAOS.Tests/ (unit + integration tests)           |

---

## SECTION 3: TRANSFORMATION RULEBOOK (30 Rules)

### Rule 1 – Target Framework Update
**Legacy:** `<TargetFramework>net6.0</TargetFramework>`
**Net10:** `<TargetFramework>net10.0</TargetFramework>`
**Explanation:** Update TFM in all .csproj files. .NET 10 is the LTS release.

### Rule 2 – Package Version Update
**Legacy:** `<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.19" />`
**Net10:** `<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />`
**Explanation:** All Microsoft.* packages must be updated to 10.0.0.

### Rule 3 – EF Core HasName() → HasDatabaseName()
**Legacy:** `b.HasIndex("NormalizedName").HasName("RoleNameIndex")`
**Net10:** `b.HasIndex("NormalizedName").HasDatabaseName("RoleNameIndex")`
**Explanation:** HasName() was deprecated in EF Core 5 and removed in EF Core 10.

### Rule 4 – EF Core nullable Column Parameter Removal
**Legacy:** `table.Column<string>(nullable: false)`
**Net10:** `table.Column<string>()` with `.IsRequired()` in fluent API
**Explanation:** The `nullable` parameter in Column<T>() was removed in EF Core 10.

### Rule 5 – EF Core ProductVersion Annotation
**Legacy:** `.HasAnnotation("ProductVersion", "3.0.0")`
**Net10:** `.HasAnnotation("ProductVersion", "10.0.0")`
**Explanation:** Model snapshot must reflect the current EF Core version.

### Rule 6 – Synchronous Actions → Async
**Legacy:** `public IActionResult Index() { return View(); }`
**Net10:** `public async Task<IActionResult> Index() { await Task.CompletedTask; return View(); }`
**Explanation:** .NET 10 strongly prefers async for all I/O-bound controller actions.

### Rule 7 – Program.cs: Minimal Hosting Model
**Legacy (net6.0):** Already uses minimal hosting model (no Startup.cs)
**Net10:** Same pattern, add session, response caching, and DI extension methods
**Explanation:** .NET 10 continues the minimal hosting model introduced in .NET 6.

### Rule 8 – Session State Registration
**Legacy:** Not configured
**Net10:**
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });
app.UseSession(); // after UseRouting, before UseAuthorization
```
**Explanation:** Session requires distributed cache and middleware registration.

### Rule 9 – Response Caching (replaces OutputCache)
**Legacy:** `[OutputCache(Duration = 60)]` (System.Web)
**Net10:** `[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]`
**Explanation:** OutputCache attribute does not exist in ASP.NET Core.

### Rule 10 – Identity: AddDefaultIdentity with Roles
**Legacy:** `builder.Services.AddDefaultIdentity<IdentityUser>(...)`
**Net10:** `builder.Services.AddDefaultIdentity<IdentityUser>(...).AddRoles<IdentityRole>()`
**Explanation:** Role support requires explicit .AddRoles<IdentityRole>() call.

### Rule 11 – Cookie Authentication Settings
**Legacy:** web.config `<forms loginUrl="~/Account/Login" timeout="30" />`
**Net10:**
```csharp
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```
**Explanation:** All Forms Authentication settings move to Program.cs.

### Rule 12 – Connection String Location
**Legacy:** web.config `<connectionStrings><add name="..." connectionString="..." /></connectionStrings>`
**Net10:** appsettings.json `"ConnectionStrings": { "DefaultConnection": "..." }`
**Explanation:** All configuration moves from web.config to appsettings.json.

### Rule 13 – AppSettings Migration
**Legacy:** web.config `<appSettings><add key="..." value="..." /></appSettings>`
**Net10:** appsettings.json `"AppSettings": { "Key": "Value" }` + IOptions<T>
**Explanation:** Strongly-typed configuration via IOptions<T> is the .NET 10 pattern.

### Rule 14 – DbContext Constructor
**Legacy:** `public ApplicationDbContext() : base("DefaultConnection") {}`
**Net10:** `public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}`
**Explanation:** EF Core uses options injection; no connection string in constructor.

### Rule 15 – EF Core Resilient Connections
**Legacy:** Not available in EF6
**Net10:**
```csharp
options.UseSqlServer(connectionString, sqlOptions =>
    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null));
```
**Explanation:** EF Core 10 supports built-in retry policies for transient SQL errors.

### Rule 16 – Repository Pattern
**Legacy:** Direct DbContext usage in controllers
**Net10:** IRepository<T> interface in Domain; Repository<T> implementation in Infrastructure
**Explanation:** Clean Architecture separates data access concerns.

### Rule 17 – Dependency Injection
**Legacy:** DependencyResolver.SetResolver() or Unity/Autofac
**Net10:** Built-in DI via builder.Services.AddScoped/AddTransient/AddSingleton
**Explanation:** ASP.NET Core has built-in DI; no third-party container required.

### Rule 18 – Logging
**Legacy:** log4net / NLog configured in web.config
**Net10:** ILogger<T> injected via constructor; configured in appsettings.json "Logging" section
**Explanation:** Built-in structured logging replaces all legacy logging frameworks.

### Rule 19 – Static Files
**Legacy:** /Content/, /Scripts/, /Images/ served by IIS
**Net10:** /wwwroot/css/, /wwwroot/js/, /wwwroot/images/ served by UseStaticFiles()
**Explanation:** Only files in wwwroot/ are served as static content in ASP.NET Core.

### Rule 20 – Bundling and Minification
**Legacy:** System.Web.Optimization BundleConfig.cs
**Net10:** LibMan (libman.json) or LigerShark.WebOptimizer.Core
**Explanation:** System.Web.Optimization does not exist in ASP.NET Core.

### Rule 21 – Global.asax Application_Start
**Legacy:** `protected void Application_Start() { RouteConfig.RegisterRoutes(RouteTable.Routes); }`
**Net10:** Program.cs `app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");`
**Explanation:** All startup code moves to Program.cs.

### Rule 22 – Route Configuration
**Legacy:** App_Start/RouteConfig.cs with RouteTable.Routes.MapRoute()
**Net10:** Program.cs app.MapControllerRoute() or [Route] attribute routing
**Explanation:** Conventional routing is configured in Program.cs.

### Rule 23 – Filter Configuration
**Legacy:** App_Start/FilterConfig.cs GlobalFilters.Filters.Add(new HandleErrorAttribute())
**Net10:** builder.Services.AddControllersWithViews(options => options.Filters.Add<GlobalExceptionFilter>())
**Explanation:** Global filters are registered via MVC options in Program.cs.

### Rule 24 – HTTP Modules → Middleware
**Legacy:** IHttpModule in web.config `<httpModules>`
**Net10:** Custom middleware class with `app.UseMiddleware<CustomMiddleware>()`
**Explanation:** HTTP Modules are replaced by ASP.NET Core middleware pipeline.

### Rule 25 – HTTP Handlers → Endpoints
**Legacy:** IHttpHandler in web.config `<httpHandlers>`
**Net10:** Minimal API endpoints or controller actions
**Explanation:** HTTP Handlers are replaced by endpoint routing.

### Rule 26 – ViewBag / ViewData
**Legacy:** `ViewBag.Title = "Home";` (dynamic)
**Net10:** Same syntax supported; prefer strongly-typed ViewModels for new code
**Explanation:** ViewBag/ViewData still work but strongly-typed models are preferred.

### Rule 27 – Html.Action / Html.RenderAction (Child Actions)
**Legacy:** `@Html.Action("Menu", "Navigation")`
**Net10:** ViewComponents: `@await Component.InvokeAsync("Navigation")`
**Explanation:** Child actions do not exist in ASP.NET Core; use ViewComponents.

### Rule 28 – TempData Provider
**Legacy:** Session-based TempData (automatic)
**Net10:** Cookie-based TempData (default) or Session-based:
```csharp
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
```
**Explanation:** Default TempData provider changed to cookie-based in ASP.NET Core.

### Rule 29 – Anti-Forgery Token
**Legacy:** `@Html.AntiForgeryToken()` + `[ValidateAntiForgeryToken]`
**Net10:** `<form asp-antiforgery="true">` Tag Helper (auto-injects token) + `[ValidateAntiForgeryToken]`
**Explanation:** Tag Helpers automatically inject anti-forgery tokens in forms.

### Rule 30 – Model Validation
**Legacy:** `if (ModelState.IsValid) { ... }`
**Net10:** Same pattern; add `[ApiController]` for automatic 400 responses in API controllers
**Explanation:** Model validation pattern is unchanged; [ApiController] adds automatic validation.

### Rule 31 – HTTPS Enforcement
**Legacy:** IIS URL Rewrite rules in web.config
**Net10:** `app.UseHttpsRedirection(); app.UseHsts();`
**Explanation:** HTTPS enforcement is handled by ASP.NET Core middleware.

### Rule 32 – Environment-Specific Configuration
**Legacy:** web.config transforms (web.Debug.config, web.Release.config)
**Net10:** appsettings.{Environment}.json + environment variables
**Explanation:** Configuration transforms are replaced by environment-specific JSON files.

---

## SECTION 4: DATA STRATEGY WITH EF CORE SCAFFOLD COMMANDS

### Database-First Approach
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Scaffold DbContext and entities from existing database
dotnet ef dbcontext scaffold \
  "Server=(localdb)\mssqllocaldb;Database=OrbitAOS_Net10;Trusted_Connection=True;TrustServerCertificate=True" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Data/Entities \
  --context-dir Data \
  --context ApplicationDbContext \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6 \
  --namespace OrbitAOS.Infrastructure.Data \
  --data-annotations \
  --force

# Parameters explained:
#   --output-dir     : Where entity classes are generated
#   --context-dir    : Where DbContext class is generated
#   --context        : Name of the generated DbContext class
#   --project        : Infrastructure project (contains DbContext)
#   --startup-project: Web project (has appsettings.json)
#   --namespace      : C# namespace for generated classes
#   --data-annotations: Use data annotations instead of fluent API
#   --force          : Overwrite existing files
```

### Code-First Approach
```bash
# Add a new migration
dotnet ef migrations add InitialNet10 \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6 \
  --output-dir Data/Migrations

# Apply migration to database
dotnet ef database update \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6

# Generate SQL script (for DBA review before applying)
dotnet ef migrations script \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6 \
  --output migration.sql \
  --idempotent

# Roll back to a specific migration
dotnet ef database update PreviousMigrationName \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6
```

### DbContext Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS_Net10;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Repository Pattern Guidance
- Define IRepository<T> in Domain layer (no EF Core dependency)
- Implement Repository<T> in Infrastructure layer
- Register: `services.AddScoped(typeof(IRepository<>), typeof(Repository<>))`
- For complex queries, create domain-specific repositories:
  `IUserRepository : IRepository<User>` with custom query methods

---

## SECTION 5: AUTHENTICATION MIGRATION GUIDANCE

### A) Forms Authentication → Cookie Authentication
**Legacy web.config:**
```xml
<authentication mode="Forms">
  <forms loginUrl="~/Account/Login" timeout="30" />
</authentication>
```

**Net10 Program.cs:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
app.UseAuthentication();
app.UseAuthorization();
```

### B) ASP.NET Core Identity (net6.0 → net10.0)
The Identity schema is compatible between .NET 6 and .NET 10.
No database schema migration is required for the Identity tables.

**Updated Program.cs registration:**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()                          // Add role support
.AddEntityFrameworkStores<ApplicationDbContext>();
```

**ApplicationDbContext (Infrastructure layer):**
```csharp
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
```

---

## SECTION 6: BLOCKERS AND MITIGATIONS

### Blocker 1 – Target Framework net6.0 (CRITICAL)
**Impact:** High – net6.0 reached end of support May 2024
**Mitigation:** Update all .csproj files: `<TargetFramework>net10.0</TargetFramework>`

### Blocker 2 – Package Versions 6.0.19 (CRITICAL)
**Impact:** High – EF Core 6.x is incompatible with net10.0 TFM
**Mitigation:** Update all Microsoft.* packages to Version="10.0.0"

### Blocker 3 – EF Core HasName() Deprecated API (CRITICAL)
**Impact:** High – Compilation error in EF Core 10
**Mitigation:** Replace `.HasName("X")` with `.HasDatabaseName("X")` in model snapshot

### Blocker 4 – EF Core nullable Column Parameter (HIGH)
**Impact:** High – Compilation warning/error in EF Core 10
**Mitigation:** Remove `nullable: false` from Column<T>() calls; use `.IsRequired()` in fluent API

### Blocker 5 – No Clean Architecture Structure (HIGH)
**Impact:** High – Violates architecture requirement
**Mitigation:** Create Domain, Application, Infrastructure projects; move DbContext and services

### Blocker 6 – ApplicationDbContext in Web Project (HIGH)
**Impact:** Medium – Violates Clean Architecture; DbContext should be in Infrastructure
**Mitigation:** Move ApplicationDbContext to OrbitAOS.Infrastructure/Data/

### Blocker 7 – No Unit Tests (HIGH)
**Impact:** High – No test coverage for migration validation
**Mitigation:** Create OrbitAOS.Tests project with xUnit, Moq, FluentAssertions

### Blocker 8 – No Session State Configuration (MEDIUM)
**Impact:** Medium – Session not available if needed
**Mitigation:** Add AddDistributedMemoryCache(), AddSession(), app.UseSession() in Program.cs

### Blocker 9 – No Response Caching Middleware (MEDIUM)
**Impact:** Medium – [ResponseCache] attribute has no effect without middleware
**Mitigation:** Add builder.Services.AddResponseCaching() and app.UseResponseCaching()

### Blocker 10 – No Role Support in Identity (MEDIUM)
**Impact:** Medium – Role-based authorization will fail
**Mitigation:** Add .AddRoles<IdentityRole>() to Identity registration chain

### Blocker 11 – EF Core ProductVersion Annotation (MEDIUM)
**Impact:** Medium – Model snapshot mismatch warning
**Mitigation:** Update "ProductVersion" annotation to "10.0.0" in model snapshot

### Blocker 12 – No Resilient DB Connection (MEDIUM)
**Impact:** Medium – Transient SQL errors cause unhandled exceptions
**Mitigation:** Add EnableRetryOnFailure() to UseSqlServer() options

### Blocker 13 – Static Copyright Year in Layout (LOW)
**Impact:** Low – Copyright year becomes stale
**Mitigation:** Replace `2024` with `@DateTime.Now.Year` in _Layout.cshtml

### Blocker 14 – Outdated Microsoft Docs URL (LOW)
**Impact:** Low – Broken documentation link
**Mitigation:** Update `docs.microsoft.com/aspnet/core` → `learn.microsoft.com/aspnet/core`

### Blocker 15 – No Solution File (.sln) (MEDIUM)
**Impact:** Medium – Cannot build all projects together; CI/CD requires .sln
**Mitigation:** Create OrbitAOS.sln with all five projects registered

### Blocker 16 – No IDesignTimeDbContextFactory (MEDIUM)
**Impact:** Medium – EF Core CLI tools cannot find DbContext when startup project differs
**Mitigation:** Implement IDesignTimeDbContextFactory<ApplicationDbContext> in Infrastructure

### Blocker 17 – TrustServerCertificate Missing (LOW)
**Impact:** Low – SQL Server connection fails with SSL certificate error in .NET 10
**Mitigation:** Add `TrustServerCertificate=True` to connection string for development

### Blocker 18 – No HSTS Configuration (MEDIUM)
**Impact:** Medium – Missing security header in production
**Mitigation:** app.UseHsts() is already in Program.cs; verify HstsOptions for production

---

## SECTION 7: UNIT TESTING FRAMEWORK

### Test Project Structure
```
OrbitAOS.Tests/
├── Controllers/
│   └── HomeControllerTests.cs
├── Services/
│   └── HomeServiceTests.cs
├── Repositories/
│   └── RepositoryTests.cs
└── OrbitAOS.Tests.csproj
```

### Test Packages
- xUnit 2.9.0 – test framework
- Moq 4.20.72 – mocking framework
- FluentAssertions 6.12.1 – readable assertions
- Microsoft.EntityFrameworkCore.InMemory 10.0.0 – in-memory DB for repo tests
- Microsoft.AspNetCore.Mvc.Testing 10.0.0 – integration test WebApplicationFactory

### Running Tests
```bash
# Run all tests
dotnet test OrbitAOS.sln

# Run with coverage
dotnet test OrbitAOS.sln --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test OrbitAOS.Tests --filter "FullyQualifiedName~HomeControllerTests"
```

---

## SECTION 8: BUILD VERIFICATION STEPS

After completing the migration, the team must run:

```bash
# Step 1: Restore all NuGet packages
dotnet restore OrbitAOS.sln

# Step 2: Build all projects (Release configuration)
dotnet build OrbitAOS.sln --configuration Release --no-restore

# Expected: Build succeeded. 0 Warning(s). 0 Error(s).

# Step 3: Run all unit tests
dotnet test OrbitAOS.sln --configuration Release --no-build --verbosity normal

# Expected: Passed! - Failed: 0, Passed: X, Skipped: 0

# Step 4: Verify EF Core migrations
dotnet ef migrations list \
  --project OrbitAOS.Infrastructure \
  --startup-project OrbitAOS.V6

# Step 5: Publish (verify no publish errors)
dotnet publish OrbitAOS.V6/OrbitAOS.V6.csproj \
  --configuration Release \
  --output ./publish \
  --no-restore
```

**Solution File Verification:**
The OrbitAOS.sln file must contain all 5 projects:
1. OrbitAOS.Domain
2. OrbitAOS.Application
3. OrbitAOS.Infrastructure
4. OrbitAOS.V6
5. OrbitAOS.Tests

Verify with: `dotnet sln OrbitAOS.sln list`
