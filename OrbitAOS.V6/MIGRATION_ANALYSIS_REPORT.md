# ASP.NET Core MVC Migration Analysis Report
## OrbitAOS.V6 — net6.0 → .NET 10 Core MVC

**Analysis Date:** 2025-07-14  
**Analysis ID:** mvc-migration-analysis-2025-07-14  
**Source Framework:** ASP.NET Core MVC on .NET 6.0  
**Target Framework:** ASP.NET Core MVC on .NET 10  
**Module Path:** OrbitAOS.V6  
**Migration Complexity:** Medium  

---

## EXECUTIVE SUMMARY

The **OrbitAOS.V6** project is an ASP.NET Core MVC application currently targeting **net6.0** (not legacy ASP.NET MVC on .NET Framework). It uses ASP.NET Core Identity with Entity Framework Core 6.x for SQL Server. The migration path is from **.NET 6 → .NET 10**, which is a supported in-place upgrade path with no System.Web dependencies.

**Key Findings:**
- **Total Issues:** 22
- **Critical:** 2
- **High:** 6
- **Medium:** 9
- **Low:** 5
- **Estimated Effort:** 12–20 hours
- **Compatibility Score:** 68/100

The primary blockers are:
1. All NuGet packages pinned to version `6.0.x` — must be upgraded to .NET 10-compatible versions.
2. EF Core migration files use deprecated `nullable: false` / `maxLength:` column API syntax removed in EF Core 7+.
3. `ApplicationDbContextModelSnapshot.cs` references `ProductVersion = "3.0.0"` — stale snapshot must be regenerated.
4. `_Layout.cshtml` references a scoped CSS file (`OrbitAOS.V6.styles.css`) that requires the CSS Isolation feature to remain enabled in the new TFM.
5. `launchSettings.json` uses legacy IIS Express profile that may need updating for .NET 10 tooling.

---

## SECTION 1 — STEP-BY-STEP MIGRATION PLAN

### Phase 1 — Pre-Migration Assessment (Week 1)
**Goal:** Understand the full scope of changes required before touching any code.

**Tasks:**
1. Run `dotnet outdated` (read-only) against the project to list all package version gaps.
2. Review all EF Core migration files for deprecated API usage.
3. Audit `appsettings.json` connection strings for LocalDB vs. production SQL Server.
4. Confirm .NET 10 SDK is installed: `dotnet --version` (must be `10.x.x`).
5. Create a Git branch: `git checkout -b migrate/net10`.
6. Document all current routes, Identity configuration, and middleware order.

**Deliverables:**
- Package upgrade list
- EF Core migration audit
- Git branch ready

**Validation Checkpoint:** All issues catalogued; no code changes yet.

---

### Phase 2 — Environment Setup (Week 1)
**Goal:** Prepare the development environment for .NET 10.

**Tasks:**
1. Install .NET 10 SDK from https://dot.net/download.
2. Install/update EF Core CLI tools:
   ```
   dotnet tool install --global dotnet-ef --version 10.*
   ```
3. Update IDE (Visual Studio 2022 17.12+ or VS Code with C# Dev Kit).
4. Verify SDK: `dotnet --list-sdks`.

**Deliverables:**
- .NET 10 SDK installed
- EF Core tools updated

---

### Phase 3 — Core Infrastructure Migration (Week 2)
**Goal:** Update the project file and all NuGet packages to .NET 10-compatible versions.

**Tasks:**
1. Update `OrbitAOS.V6.csproj`:
   - Change `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`
   - Update all `PackageReference` versions to `10.0.x` (or latest compatible).
2. Run `dotnet restore` to verify package resolution.
3. Run `dotnet build` to identify any compilation errors.
4. Fix any breaking API changes surfaced by the build.

**Before (csproj):**
```xml
<TargetFramework>net6.0</TargetFramework>
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="6.0.19" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.19" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="6.0.19" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.19" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.19" />
```

**After (csproj):**
```xml
<TargetFramework>net10.0</TargetFramework>
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
```

**Deliverables:**
- Updated `.csproj`
- Successful `dotnet restore`

---

### Phase 4 — Data Layer Migration (Week 2–3)
**Goal:** Fix EF Core migration files and regenerate the model snapshot.

**Tasks:**
1. Delete the stale migration file `00000000000000_CreateIdentitySchema.cs` and its Designer.
2. Delete `ApplicationDbContextModelSnapshot.cs`.
3. Regenerate the Identity schema migration:
   ```
   dotnet ef migrations add CreateIdentitySchema --output-dir Data/Migrations
   ```
4. Fix deprecated `nullable: false` column syntax in migration files (see Issue #5).
5. Update `ApplicationDbContext` if any EF Core 7/8/9/10 breaking changes apply.
6. Run `dotnet ef database update` against a dev database to verify.

**Deliverables:**
- Regenerated migration files
- Updated model snapshot with `ProductVersion = "10.0.x"`

---

### Phase 5 — Controllers and Models Migration (Week 3)
**Goal:** Verify controllers and models compile and function correctly under .NET 10.

**Tasks:**
1. Review `HomeController.cs` — no changes required (already uses `IActionResult`, DI, `ILogger`).
2. Review `ErrorViewModel.cs` — no changes required (nullable reference types already enabled).
3. Verify `[ResponseCache]` attribute behavior is unchanged in .NET 10.
4. Add any new controllers following the existing pattern.

**Deliverables:**
- All controllers compile under net10.0

---

### Phase 6 — Views Migration (Week 3)
**Goal:** Update Razor views for .NET 10 compatibility.

**Tasks:**
1. Update `_Layout.cshtml` — verify `OrbitAOS.V6.styles.css` CSS Isolation still works.
2. Update `_ViewImports.cshtml` — no changes required.
3. Update `_LoginPartial.cshtml` — verify Identity UI Razor Pages still scaffold correctly.
4. Update `Index.cshtml` — update Microsoft docs link from `docs.microsoft.com/aspnet/core` to `learn.microsoft.com/aspnet/core`.
5. Verify `_ValidationScriptsPartial.cshtml` jQuery validation library versions.

**Deliverables:**
- All views render correctly

---

### Phase 7 — Authentication and Authorization Migration (Week 3–4)
**Goal:** Ensure ASP.NET Core Identity works correctly under .NET 10.

**Tasks:**
1. Verify `AddDefaultIdentity<IdentityUser>` is still the correct API in .NET 10.
2. Confirm `RequireConfirmedAccount = true` behavior is unchanged.
3. Test login, logout, register flows end-to-end.
4. Review Identity UI scaffolded pages under `Areas/Identity/Pages/`.
5. Verify `_LoginPartial.cshtml` `SignInManager` and `UserManager` injection still works.

**Deliverables:**
- Authentication flows tested and working

---

### Phase 8 — Advanced Features Migration (Week 4)
**Goal:** Address remaining compatibility issues.

**Tasks:**
1. Update `launchSettings.json` for .NET 10 tooling compatibility.
2. Update `appsettings.json` connection string from LocalDB to production SQL Server.
3. Review `Program.cs` middleware pipeline for any deprecated APIs in .NET 10.
4. Verify `app.UseMigrationsEndPoint()` is still available in .NET 10.
5. Update static file serving if any changes in .NET 10.

**Deliverables:**
- All configuration files updated

---

### Phase 9 — Testing and Validation (Week 4–5)
**Goal:** Verify the migrated application works correctly.

**Tasks:**
1. Create a test project: `dotnet new xunit -n OrbitAOS.V6.Tests`
2. Add test project to solution.
3. Write unit tests for `HomeController`.
4. Write integration tests for Identity flows.
5. Run all tests: `dotnet test`.
6. Perform manual smoke testing of all routes.

**Deliverables:**
- Test project with passing tests
- All routes verified

---

### Phase 10 — Deployment and Cutover (Week 5–6)
**Goal:** Deploy the migrated application to production.

**Tasks:**
1. Create solution file: `dotnet new sln -n OrbitAOS`
2. Add project to solution: `dotnet sln add OrbitAOS.V6/OrbitAOS.V6.csproj`
3. Run final build verification: `dotnet build OrbitAOS.sln`
4. Publish: `dotnet publish -c Release -o ./publish`
5. Deploy to staging environment.
6. Run smoke tests on staging.
7. Blue-green deployment to production.
8. Monitor for 48 hours.

**Build Verification (team must run after migration):**
```bash
# Step 1: Create solution file
dotnet new sln -n OrbitAOS

# Step 2: Add project to solution
dotnet sln OrbitAOS.sln add OrbitAOS.V6/OrbitAOS.V6.csproj

# Step 3: Restore packages
dotnet restore OrbitAOS.sln

# Step 4: Build solution (must produce 0 errors, 0 warnings)
dotnet build OrbitAOS.sln --configuration Release

# Step 5: Run tests
dotnet test OrbitAOS.sln

# Expected output:
# Build succeeded.
# 0 Error(s)
# 0 Warning(s)
```

**Deliverables:**
- Solution file (`OrbitAOS.sln`)
- Successful `dotnet build` with 0 errors
- Production deployment

---

## SECTION 2 — FOLDER-BY-FOLDER MAPPING

| Legacy / Current (net6.0) | Target (.NET 10) | Notes |
|---|---|---|
| `Controllers/` | `Controllers/` | No structural change |
| `Views/` | `Views/` | No structural change |
| `Views/_ViewImports.cshtml` | `Views/_ViewImports.cshtml` | No change needed |
| `Views/_ViewStart.cshtml` | `Views/_ViewStart.cshtml` | No change needed |
| `Views/Shared/_Layout.cshtml` | `Views/Shared/_Layout.cshtml` | Minor updates |
| `Views/Shared/_LoginPartial.cshtml` | `Views/Shared/_LoginPartial.cshtml` | Verify Identity UI |
| `Models/` | `Models/` | No structural change |
| `Data/` | `Data/` | Regenerate migrations |
| `Data/Migrations/` | `Data/Migrations/` | Delete and regenerate |
| `Areas/Identity/Pages/` | `Areas/Identity/Pages/` | No structural change |
| `wwwroot/` | `wwwroot/` | No structural change |
| `wwwroot/lib/` | `wwwroot/lib/` | Update library versions |
| `Properties/launchSettings.json` | `Properties/launchSettings.json` | Minor updates |
| `appsettings.json` | `appsettings.json` | Update connection string |
| `Program.cs` | `Program.cs` | Minor updates |
| `OrbitAOS.V6.csproj` | `OrbitAOS.V6.csproj` | **Major update required** |
| *(missing)* | `OrbitAOS.sln` | **Create new solution file** |
| *(missing)* | `OrbitAOS.V6.Tests/` | **Create test project** |

---

## SECTION 3 — TRANSFORMATION RULEBOOK (30+ Rules)

### Rule 1 — Target Framework Update
**Category:** Core Framework & Startup  
**Legacy (.NET 6):**
```xml
<TargetFramework>net6.0</TargetFramework>
```
**Target (.NET 10):**
```xml
<TargetFramework>net10.0</TargetFramework>
```
**Explanation:** The TFM (Target Framework Moniker) must be updated to `net10.0`. This is the single most important change that unlocks all .NET 10 features and package compatibility.

---

### Rule 2 — Package Version: Diagnostics EntityFrameworkCore
**Category:** Package Compatibility  
**Legacy:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="6.0.19" />
```
**Target:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.0" />
```
**Explanation:** Must match the target framework version. Version 6.x is not compatible with net10.0.

---

### Rule 3 — Package Version: Identity EntityFrameworkCore
**Category:** Package Compatibility  
**Legacy:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.19" />
```
**Target:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
```
**Explanation:** Identity EF Core package must be updated to match the target framework.

---

### Rule 4 — Package Version: Identity UI
**Category:** Package Compatibility  
**Legacy:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="6.0.19" />
```
**Target:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.0" />
```
**Explanation:** Identity UI scaffolded pages are bundled in this package. Must match target framework.

---

### Rule 5 — Package Version: EF Core SQL Server
**Category:** Package Compatibility  
**Legacy:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.19" />
```
**Target:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
```
**Explanation:** EF Core SQL Server provider must match the target framework version.

---

### Rule 6 — Package Version: EF Core Tools
**Category:** Package Compatibility  
**Legacy:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.19" />
```
**Target:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
```
**Explanation:** EF Core Tools must match the target framework for `dotnet ef` commands to work.

---

### Rule 7 — EF Core Migration: Deprecated `nullable` Column Parameter
**Category:** Data Access  
**Legacy (EF Core 3.x/5.x/6.x syntax):**
```csharp
Id = table.Column<string>(nullable: false),
Name = table.Column<string>(maxLength: 256, nullable: true),
```
**Target (EF Core 7+ syntax):**
```csharp
Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
```
**Explanation:** EF Core 7 changed the column definition API. The `nullable:` named parameter is still supported but the `type:` parameter is now preferred. Regenerating migrations with EF Core 10 tools produces the correct syntax automatically.

---

### Rule 8 — EF Core Migration: Deprecated `HasName` Index API
**Category:** Data Access  
**Legacy:**
```csharp
b.HasIndex("NormalizedName")
    .IsUnique()
    .HasName("RoleNameIndex")
    .HasFilter("[NormalizedName] IS NOT NULL");
```
**Target:**
```csharp
b.HasIndex("NormalizedName")
    .IsUnique()
    .HasDatabaseName("RoleNameIndex")
    .HasFilter("[NormalizedName] IS NOT NULL");
```
**Explanation:** `HasName()` was renamed to `HasDatabaseName()` in EF Core 5+. The old API is obsolete and removed in EF Core 7+.

---

### Rule 9 — EF Core Snapshot: ProductVersion Annotation
**Category:** Data Access  
**Legacy:**
```csharp
modelBuilder
    .HasAnnotation("ProductVersion", "3.0.0")
    .HasAnnotation("Relational:MaxIdentifierLength", 128)
    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
```
**Target (regenerated by EF Core 10 tools):**
```csharp
modelBuilder
    .HasAnnotation("ProductVersion", "10.0.0")
    .HasAnnotation("Relational:MaxIdentifierLength", 128);
```
**Explanation:** The model snapshot must be regenerated with EF Core 10 tools. The stale `ProductVersion = "3.0.0"` causes migration conflicts.

---

### Rule 10 — EF Core: SqlServerValueGenerationStrategy Annotation
**Category:** Data Access  
**Legacy:**
```csharp
.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
```
**Target:**
```csharp
.UseIdentityColumn()
```
**Explanation:** In EF Core 7+, the fluent API `UseIdentityColumn()` replaces the raw annotation. Regenerating migrations handles this automatically.

---

### Rule 11 — Program.cs: UseMigrationsEndPoint Availability
**Category:** Core Framework & Startup  
**Legacy:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
```
**Target (.NET 10 — same, but verify package):**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
```
**Explanation:** `UseMigrationsEndPoint()` is provided by `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`. As long as that package is updated to 10.0.x, this call remains valid.

---

### Rule 12 — Program.cs: AddDefaultIdentity with Roles
**Category:** Authentication & Security  
**Legacy:**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```
**Target (if roles are needed):**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```
**Explanation:** The current configuration does not include roles. If role-based authorization is needed, `.AddRoles<IdentityRole>()` must be added. The base pattern is unchanged in .NET 10.

---

### Rule 13 — Program.cs: Middleware Order Verification
**Category:** Core Framework & Startup  
**Legacy (correct order):**
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(...);
app.MapRazorPages();
```
**Target (.NET 10 — same order, verified):**
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(...);
app.MapRazorPages();
```
**Explanation:** The middleware order is correct and unchanged in .NET 10. No modifications needed.

---

### Rule 14 — Views: Deprecated Microsoft Docs URL
**Category:** Views & Razor  
**Legacy (`Index.cshtml`):**
```html
<p>Learn about <a href="https://docs.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
```
**Target:**
```html
<p>Learn about <a href="https://learn.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
```
**Explanation:** Microsoft migrated all documentation from `docs.microsoft.com` to `learn.microsoft.com`. The old URL redirects but should be updated.

---

### Rule 15 — Views: CSS Isolation File Reference
**Category:** Views & Razor  
**Legacy (`_Layout.cshtml`):**
```html
<link rel="stylesheet" href="~/OrbitAOS.V6.styles.css" asp-append-version="true" />
```
**Target (.NET 10 — same, verify build output):**
```html
<link rel="stylesheet" href="~/OrbitAOS.V6.styles.css" asp-append-version="true" />
```
**Explanation:** CSS Isolation (scoped CSS) is supported in .NET 10. The `OrbitAOS.V6.styles.css` file is auto-generated at build time from `*.razor.css` / `*.cshtml.css` files. Verify `Views/Shared/_Layout.cshtml.css` is included in the build.

---

### Rule 16 — Views: _LoginPartial Identity UI Compatibility
**Category:** Authentication & Security  
**Legacy:**
```csharp
@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager
```
**Target (.NET 10 — same):**
```csharp
@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager
```
**Explanation:** The `SignInManager<T>` and `UserManager<T>` injection pattern is unchanged in .NET 10. No modifications needed.

---

### Rule 17 — Views: Tag Helper Namespace in _ViewImports
**Category:** Views & Razor  
**Legacy:**
```csharp
@using OrbitAOS.V6
@using OrbitAOS.V6.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
**Target (.NET 10 — same):**
```csharp
@using OrbitAOS.V6
@using OrbitAOS.V6.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
**Explanation:** Tag Helper registration is unchanged in .NET 10. No modifications needed.

---

### Rule 18 — launchSettings.json: Profile Update
**Category:** Configuration  
**Legacy:**
```json
"applicationUrl": "https://localhost:7073;http://localhost:5008"
```
**Target (.NET 10 — update ports if needed):**
```json
"applicationUrl": "https://localhost:7073;http://localhost:5008"
```
**Explanation:** Port configuration is unchanged. However, verify that IIS Express profile is still supported in .NET 10 tooling. The Kestrel profile (`OrbitAOS.V6`) is the recommended approach.

---

### Rule 19 — appsettings.json: Connection String for Production
**Category:** Configuration  
**Legacy (LocalDB):**
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-OrbitAOS.V6-afe03967-a771-44f8-b4ff-ee887e025b4f;Trusted_Connection=True;MultipleActiveResultSets=true"
```
**Target (Production SQL Server):**
```json
"DefaultConnection": "Server=your-server;Database=OrbitAOS;User Id=your-user;Password=your-password;TrustServerCertificate=True;"
```
**Explanation:** LocalDB is for development only. Production deployments must use a proper SQL Server connection string. Use User Secrets or environment variables for sensitive values.

---

### Rule 20 — appsettings.json: MultipleActiveResultSets Deprecation
**Category:** Configuration  
**Legacy:**
```json
"MultipleActiveResultSets=true"
```
**Target (remove or keep):**
```json
// Remove MARS if not needed; EF Core does not require it
"Server=(localdb)\\mssqllocaldb;Database=OrbitAOS;Trusted_Connection=True;"
```
**Explanation:** `MultipleActiveResultSets=true` (MARS) is not required by EF Core. It can cause performance issues. Remove it unless explicitly needed.

---

### Rule 21 — Controller: IActionResult Return Type
**Category:** Controllers & Actions  
**Legacy (already correct):**
```csharp
public IActionResult Index()
{
    return View();
}
```
**Target (.NET 10 — same, or use typed results):**
```csharp
// Option 1: Keep IActionResult (no change needed)
public IActionResult Index() => View();

// Option 2: Use typed results (new in .NET 7+)
public ViewResult Index() => View();
```
**Explanation:** `IActionResult` is unchanged. Optionally use more specific return types for better testability.

---

### Rule 22 — Controller: Async Action Methods
**Category:** Controllers & Actions  
**Legacy (synchronous):**
```csharp
public IActionResult Index()
{
    return View();
}
```
**Target (.NET 10 — async preferred for I/O):**
```csharp
public async Task<IActionResult> Index()
{
    // await async operations
    return View();
}
```
**Explanation:** For actions that perform I/O (database, HTTP calls), use async/await. The current `HomeController` has no I/O so synchronous is acceptable.

---

### Rule 23 — Controller: ResponseCache Attribute
**Category:** Controllers & Actions  
**Legacy (already correct):**
```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error()
```
**Target (.NET 10 — same):**
```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error()
```
**Explanation:** `[ResponseCache]` attribute is unchanged in .NET 10. No modifications needed.

---

### Rule 24 — Model: Nullable Reference Types
**Category:** Controllers & Actions  
**Legacy (already correct):**
```csharp
public string? RequestId { get; set; }
public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
```
**Target (.NET 10 — same):**
```csharp
public string? RequestId { get; set; }
public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
```
**Explanation:** Nullable reference types (`<Nullable>enable</Nullable>`) are already enabled. No changes needed.

---

### Rule 25 — Data: ApplicationDbContext Base Class
**Category:** Data Access  
**Legacy (already correct):**
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```
**Target (.NET 10 — same):**
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```
**Explanation:** `IdentityDbContext` base class is unchanged in .NET 10. No modifications needed.

---

### Rule 26 — Data: EF Core Connection String Configuration
**Category:** Data Access  
**Legacy:**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```
**Target (.NET 10 — same, but add null check):**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```
**Explanation:** In .NET 10, `GetConnectionString()` returns `string?`. Adding a null check prevents runtime errors and is the recommended pattern.

---

### Rule 27 — Security: HSTS Configuration
**Category:** Authentication & Security  
**Legacy:**
```csharp
app.UseHsts();
```
**Target (.NET 10 — same, but consider max-age):**
```csharp
// In Program.cs, configure HSTS options
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
app.UseHsts();
```
**Explanation:** The default HSTS max-age is 30 days. For production, increase to 365 days and consider preloading.

---

### Rule 28 — Security: Anti-Forgery Token
**Category:** Authentication & Security  
**Legacy (implicit in forms):**
```html
<form asp-area="Identity" asp-page="/Account/Logout" ...>
    <button type="submit">Logout</button>
</form>
```
**Target (.NET 10 — same, verify AntiForgery middleware):**
```csharp
// In Program.cs, AntiForgery is registered automatically with AddControllersWithViews()
// No explicit configuration needed
builder.Services.AddControllersWithViews();
```
**Explanation:** Anti-forgery token validation is automatic with Tag Helpers in ASP.NET Core. No changes needed.

---

### Rule 29 — Routing: MapRazorPages for Identity UI
**Category:** Core Framework & Startup  
**Legacy (already correct):**
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
```
**Target (.NET 10 — same):**
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
```
**Explanation:** `MapRazorPages()` is required for Identity UI Razor Pages. The order (after `MapControllerRoute`) is correct. No changes needed.

---

### Rule 30 — Static Files: wwwroot Structure
**Category:** Views & Razor  
**Legacy (already correct):**
```
wwwroot/
  css/site.css
  js/site.js
  lib/bootstrap/
  lib/jquery/
  lib/jquery-validation/
  lib/jquery-validation-unobtrusive/
```
**Target (.NET 10 — same structure, update library versions):**
```
wwwroot/
  css/site.css
  js/site.js
  lib/bootstrap/          # Consider updating to Bootstrap 5.3+
  lib/jquery/             # Consider updating to jQuery 3.7+
  lib/jquery-validation/
  lib/jquery-validation-unobtrusive/
```
**Explanation:** The `wwwroot` structure is unchanged. Consider updating client-side library versions using LibMan (`libman.json`).

---

### Rule 31 — Testing: Unit Test Project Setup
**Category:** Testing  
**Legacy (no test project):**
```
(no test project exists)
```
**Target (.NET 10):**
```bash
# Create test project
dotnet new xunit -n OrbitAOS.V6.Tests --framework net10.0

# Add to solution
dotnet sln OrbitAOS.sln add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj

# Add project reference
dotnet add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj reference OrbitAOS.V6/OrbitAOS.V6.csproj

# Add test packages
dotnet add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj package Moq
dotnet add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
```
**Explanation:** A test project is required for CI/CD and quality assurance. Use xUnit with Moq for unit tests and `Microsoft.AspNetCore.Mvc.Testing` for integration tests.

---

### Rule 32 — Testing: HomeController Unit Test
**Category:** Testing  
**Legacy (no tests):**
```
(no tests)
```
**Target (.NET 10):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using Xunit;

namespace OrbitAOS.V6.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_loggerMock.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        var result = _controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var result = _controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResultWithErrorViewModel()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = _controller.Error() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ErrorViewModel>(result.Model);
    }
}
```
**Explanation:** Basic unit tests for the `HomeController` using xUnit and Moq.

---

### Rule 33 — Solution File Creation
**Category:** Core Framework & Startup  
**Legacy (no solution file in module):**
```
(no .sln file)
```
**Target (.NET 10):**
```bash
# Create solution file at workspace root
dotnet new sln -n OrbitAOS --output /path/to/workspace

# Add main project
dotnet sln OrbitAOS.sln add OrbitAOS.V6/OrbitAOS.V6.csproj

# Add test project (after creating it)
dotnet sln OrbitAOS.sln add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj
```
**Explanation:** A solution file is required for multi-project builds and IDE support. The team must create this after migration.

---

### Rule 34 — Configuration: User Secrets for Development
**Category:** Configuration  
**Legacy (UserSecretsId in csproj):**
```xml
<UserSecretsId>aspnet-OrbitAOS.V6-afe03967-a771-44f8-b4ff-ee887e025b4f</UserSecretsId>
```
**Target (.NET 10 — same, use for connection strings):**
```bash
# Store connection string in user secrets (development only)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;"
```
**Explanation:** The `UserSecretsId` is already configured. Use `dotnet user-secrets` to store sensitive configuration values in development instead of `appsettings.json`.

---

### Rule 35 — Logging: Structured Logging
**Category:** Core Framework & Startup  
**Legacy:**
```csharp
private readonly ILogger<HomeController> _logger;
public HomeController(ILogger<HomeController> logger)
{
    _logger = logger;
}
```
**Target (.NET 10 — same, add structured logging):**
```csharp
// Use structured logging with named parameters
_logger.LogInformation("Processing request for {Action}", nameof(Index));

// Use LoggerMessage source generator for high-performance logging
[LoggerMessage(Level = LogLevel.Information, Message = "Processing {Action}")]
private static partial void LogAction(ILogger logger, string action);
```
**Explanation:** The DI-based `ILogger<T>` pattern is unchanged. Consider using `LoggerMessage` source generators for high-performance logging in .NET 10.

---

## SECTION 4 — DATA STRATEGY WITH EF CORE SCAFFOLD COMMANDS

### Current State
The project uses EF Core 6.x with a single migration (`00000000000000_CreateIdentitySchema`) that creates the ASP.NET Core Identity schema. The migration files contain deprecated API syntax from EF Core 3.0.

### Recommended Approach: Regenerate Migrations

**Step 1: Delete stale migration files**
```bash
# Delete the stale migration files (do NOT delete the database)
rm Data/Migrations/00000000000000_CreateIdentitySchema.cs
rm Data/Migrations/00000000000000_CreateIdentitySchema.Designer.cs
rm Data/Migrations/ApplicationDbContextModelSnapshot.cs
```

**Step 2: Update packages to EF Core 10**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
```

**Step 3: Install EF Core CLI tools**
```bash
dotnet tool install --global dotnet-ef --version 10.*
# or update existing:
dotnet tool update --global dotnet-ef --version 10.*
```

**Step 4: Regenerate the Identity schema migration**
```bash
# Parameters explained:
# migrations add     - Creates a new migration
# CreateIdentitySchema - Name of the migration
# --project          - Path to the project containing DbContext
# --startup-project  - Path to the startup project (same here)
# --output-dir       - Output directory for migration files
# --context          - DbContext class name (if multiple contexts exist)

dotnet ef migrations add CreateIdentitySchema \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --output-dir Data/Migrations \
    --context ApplicationDbContext
```

**Step 5: Apply migrations to database**
```bash
# Parameters explained:
# database update    - Applies pending migrations to the database
# --project          - Path to the project
# --startup-project  - Path to the startup project
# --context          - DbContext class name
# --connection       - Override connection string (optional)

dotnet ef database update \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --context ApplicationDbContext
```

**Step 6: Scaffold existing database (Database-First approach)**
```bash
# If you have an existing database and want to scaffold models:
# Parameters explained:
# dbcontext scaffold - Generates DbContext and entity classes from database
# "connection-string" - The database connection string
# Microsoft.EntityFrameworkCore.SqlServer - The database provider
# --output-dir       - Output directory for generated files
# --context-dir      - Output directory for DbContext
# --context          - Name of the generated DbContext class
# --data-annotations - Use data annotations instead of fluent API
# --force            - Overwrite existing files

dotnet ef dbcontext scaffold \
    "Server=(localdb)\mssqllocaldb;Database=OrbitAOS;Trusted_Connection=True;" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir Models/Entities \
    --context-dir Data \
    --context ApplicationDbContext \
    --data-annotations \
    --force \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6
```

**Step 7: Future schema changes**
```bash
# Add a new migration after model changes:
dotnet ef migrations add AddNewFeature \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --context ApplicationDbContext

# Apply the new migration:
dotnet ef database update \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --context ApplicationDbContext

# Rollback to a specific migration:
dotnet ef database update CreateIdentitySchema \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --context ApplicationDbContext

# Generate SQL script for production deployment:
dotnet ef migrations script \
    --idempotent \
    --output migration.sql \
    --project OrbitAOS.V6 \
    --startup-project OrbitAOS.V6 \
    --context ApplicationDbContext
```

### DbContext Configuration (.NET 10)
```csharp
// Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
    
    // Enable sensitive data logging in development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

### Repository Pattern (Optional)
```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}

// Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public async Task UpdateAsync(T entity) => _dbSet.Update(entity);
    public async Task DeleteAsync(T entity) => _dbSet.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

// Register in Program.cs
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

---

## SECTION 5 — AUTHENTICATION MIGRATION GUIDANCE

### Current Authentication Setup
The project uses **ASP.NET Core Identity** with:
- `AddDefaultIdentity<IdentityUser>` — basic Identity without roles
- `RequireConfirmedAccount = true` — email confirmation required
- `IdentityDbContext` — EF Core-backed Identity store
- Identity UI Razor Pages via `Microsoft.AspNetCore.Identity.UI`

### A) Current Identity → .NET 10 Identity (In-Place Upgrade)

**Current `Program.cs` (net6.0):**
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Target `Program.cs` (.NET 10):**
```csharp
// Option 1: Keep AddDefaultIdentity (no roles)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // .NET 10 recommended password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Option 2: Use AddIdentity (with roles support)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();
```

### B) Custom ApplicationUser (Recommended for Production)
```csharp
// Models/ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Data/ApplicationDbContext.cs
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

// Program.cs
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

### C) Cookie Authentication Configuration (.NET 10)
```csharp
// Program.cs — configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
```

### D) Database Schema Compatibility
The Identity schema (AspNetUsers, AspNetRoles, etc.) is **fully compatible** between EF Core 6 and EF Core 10. No schema changes are required for the standard `IdentityUser` tables. If you add custom properties to `ApplicationUser`, run a new migration.

### E) Account Controller Migration (if scaffolded)
```csharp
// If you scaffold Identity pages, the Account controller pattern becomes:
// Areas/Identity/Pages/Account/Login.cshtml.cs
public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            // Handle other results...
        }
        return Page();
    }
}
```

---

## SECTION 6 — BLOCKERS & MITIGATIONS

### Blocker 1 — All NuGet Packages Pinned to net6.0 Versions
**Impact:** Critical  
**Description:** All 5 NuGet packages are pinned to version `6.0.19`, which is incompatible with `net10.0`. The project will fail to restore and build.  
**Files:** `OrbitAOS.V6.csproj`  
**Mitigation:**
```xml
<!-- Update all packages to 10.0.x -->
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
```

---

### Blocker 2 — Target Framework Not Updated
**Impact:** Critical  
**Description:** `<TargetFramework>net6.0</TargetFramework>` must be changed to `net10.0`. Without this change, no .NET 10 features or packages are available.  
**Files:** `OrbitAOS.V6.csproj`  
**Mitigation:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

---

### Blocker 3 — Stale EF Core Migration Files (ProductVersion 3.0.0)
**Impact:** High  
**Description:** `ApplicationDbContextModelSnapshot.cs` and `00000000000000_CreateIdentitySchema.Designer.cs` both reference `ProductVersion = "3.0.0"`. This is a stale snapshot from EF Core 3.0 that will cause migration conflicts with EF Core 10.  
**Files:** `Data/Migrations/ApplicationDbContextModelSnapshot.cs` (line 17), `Data/Migrations/00000000000000_CreateIdentitySchema.Designer.cs` (line 17)  
**Mitigation:** Delete all migration files and regenerate with EF Core 10 tools:
```bash
dotnet ef migrations add CreateIdentitySchema --output-dir Data/Migrations
```

---

### Blocker 4 — Deprecated `nullable:` Column Parameter in Migration
**Impact:** High  
**Description:** The migration file uses `nullable: false` and `nullable: true` as named parameters in `table.Column<T>()`. This syntax was deprecated in EF Core 6 and removed in EF Core 7+.  
**Files:** `Data/Migrations/00000000000000_CreateIdentitySchema.cs` (lines 10–80)  
**Code:** `Id = table.Column<string>(nullable: false),`  
**Mitigation:** Regenerate migrations with EF Core 10 tools. The new syntax uses `type:` parameter.

---

### Blocker 5 — Deprecated `HasName()` Index API in Model Snapshot
**Impact:** High  
**Description:** `ApplicationDbContextModelSnapshot.cs` uses `.HasName("RoleNameIndex")` which was renamed to `.HasDatabaseName()` in EF Core 5 and removed in EF Core 7+.  
**Files:** `Data/Migrations/ApplicationDbContextModelSnapshot.cs` (lines 43, 75)  
**Code:** `.HasName("RoleNameIndex")`  
**Mitigation:** Regenerate model snapshot with EF Core 10 tools.

---

### Blocker 6 — Deprecated `SqlServerValueGenerationStrategy.IdentityColumn` Annotation
**Impact:** High  
**Description:** Migration files use `.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)` which is deprecated in EF Core 7+ in favor of `.UseIdentityColumn()`.  
**Files:** `Data/Migrations/00000000000000_CreateIdentitySchema.cs` (lines 45, 65), `Data/Migrations/ApplicationDbContextModelSnapshot.cs` (line 18)  
**Mitigation:** Regenerate migrations with EF Core 10 tools.

---

### Blocker 7 — No Solution File (.sln)
**Impact:** High  
**Description:** There is no `.sln` file in the module directory. A solution file is required for multi-project builds, CI/CD pipelines, and IDE support.  
**Mitigation:**
```bash
dotnet new sln -n OrbitAOS
dotnet sln OrbitAOS.sln add OrbitAOS.V6/OrbitAOS.V6.csproj
```

---

### Blocker 8 — No Unit Test Project
**Impact:** High  
**Description:** There is no test project in the solution. Unit tests are required for validating the migration and for CI/CD pipelines.  
**Mitigation:**
```bash
dotnet new xunit -n OrbitAOS.V6.Tests --framework net10.0
dotnet sln OrbitAOS.sln add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj
dotnet add OrbitAOS.V6.Tests/OrbitAOS.V6.Tests.csproj reference OrbitAOS.V6/OrbitAOS.V6.csproj
```

---

### Blocker 9 — Connection String Uses LocalDB (Not Production-Ready)
**Impact:** Medium  
**Description:** `appsettings.json` uses `(localdb)\mssqllocaldb` which is only available on Windows developer machines. Production deployments require a proper SQL Server connection string.  
**Files:** `appsettings.json` (line 3)  
**Mitigation:** Use environment-specific configuration:
```json
// appsettings.json (development default)
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS;Trusted_Connection=True;"

// appsettings.Production.json
"DefaultConnection": "Server=prod-server;Database=OrbitAOS;User Id=app_user;Password=...;TrustServerCertificate=True;"
```

---

### Blocker 10 — `MultipleActiveResultSets=true` in Connection String
**Impact:** Medium  
**Description:** MARS (`MultipleActiveResultSets=true`) is enabled in the connection string. EF Core does not require MARS and it can cause performance issues and unexpected behavior.  
**Files:** `appsettings.json` (line 3)  
**Mitigation:** Remove `MultipleActiveResultSets=true` from the connection string unless explicitly required.

---

### Blocker 11 — Identity Requires Email Confirmation (UX Impact)
**Impact:** Medium  
**Description:** `options.SignIn.RequireConfirmedAccount = true` requires email confirmation before login. Without an email service configured, new user registrations will be blocked.  
**Files:** `Program.cs` (line 12)  
**Mitigation:** Either configure an email service (SendGrid, SMTP) or set `RequireConfirmedAccount = false` for development:
```csharp
// Development: disable email confirmation
options.SignIn.RequireConfirmedAccount = builder.Environment.IsProduction();
```

---

### Blocker 12 — No Email Service Configured
**Impact:** Medium  
**Description:** The project uses `RequireConfirmedAccount = true` but has no email sender service registered. The default `IEmailSender` implementation is a no-op that does nothing.  
**Mitigation:** Register a real email sender:
```csharp
// Install: dotnet add package SendGrid
builder.Services.AddTransient<IEmailSender, EmailSender>();

// EmailSender.cs
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    public EmailSender(IConfiguration config) => _config = config;
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SendGridClient(_config["SendGrid:ApiKey"]);
        var msg = MailHelper.CreateSingleEmail(
            new EmailAddress(_config["SendGrid:FromEmail"]),
            new EmailAddress(email), subject, null, htmlMessage);
        await client.SendEmailAsync(msg);
    }
}
```

---

### Blocker 13 — CSS Isolation File May Not Generate Correctly
**Impact:** Medium  
**Description:** `_Layout.cshtml` references `~/OrbitAOS.V6.styles.css` which is auto-generated from `Views/Shared/_Layout.cshtml.css`. If the CSS Isolation build step fails after the TFM upgrade, the layout will be missing styles.  
**Files:** `Views/Shared/_Layout.cshtml` (line 8), `Views/Shared/_Layout.cshtml.css`  
**Mitigation:** Verify the CSS Isolation file is included in the build output after upgrading. If not, add explicit CSS Isolation configuration:
```xml
<!-- In .csproj if needed -->
<ItemGroup>
    <None Update="Views/**/*.cshtml.css" CopyToOutputDirectory="Never" />
</ItemGroup>
```

---

### Blocker 14 — IIS Express Profile May Not Work with .NET 10
**Impact:** Low  
**Description:** `launchSettings.json` includes an IIS Express profile. IIS Express support in .NET 10 tooling may require updated Visual Studio version.  
**Files:** `Properties/launchSettings.json`  
**Mitigation:** Use the Kestrel profile (`OrbitAOS.V6`) for development. IIS Express is not required for .NET 10 development.

---

### Blocker 15 — Deprecated Microsoft Docs URL in View
**Impact:** Low  
**Description:** `Views/Home/Index.cshtml` links to `docs.microsoft.com/aspnet/core` which has been migrated to `learn.microsoft.com/aspnet/core`.  
**Files:** `Views/Home/Index.cshtml` (line 5)  
**Mitigation:**
```html
<a href="https://learn.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>
```

---

### Blocker 16 — No HTTPS Certificate for Production
**Impact:** Medium  
**Description:** The project uses `app.UseHttpsRedirection()` and `app.UseHsts()` but has no production HTTPS certificate configuration.  
**Mitigation:** Configure HTTPS certificate in production:
```json
// appsettings.Production.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "cert-password"
        }
      }
    }
  }
}
```

---

### Blocker 17 — No Health Check Endpoint
**Impact:** Low  
**Description:** The project has no health check endpoint, which is required for container orchestration (Kubernetes, Docker) and load balancer health probes.  
**Mitigation:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

---

### Blocker 18 — No Logging Configuration for Production
**Impact:** Low  
**Description:** `appsettings.json` only configures console logging. Production deployments need structured logging to a log aggregator.  
**Mitigation:**
```csharp
// Program.cs — add Serilog or Application Insights
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

---

## SECTION 7 — UNIT TESTING FRAMEWORK IMPLEMENTATION

### Test Project Structure
```
OrbitAOS.V6.Tests/
├── OrbitAOS.V6.Tests.csproj
├── Controllers/
│   └── HomeControllerTests.cs
├── Data/
│   └── ApplicationDbContextTests.cs
├── Integration/
│   └── WebApplicationTests.cs
└── Helpers/
    └── TestDbContextFactory.cs
```

### Test Project Configuration
```xml
<!-- OrbitAOS.V6.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../OrbitAOS.V6/OrbitAOS.V6.csproj" />
  </ItemGroup>
</Project>
```

### HomeController Tests
```csharp
// Controllers/HomeControllerTests.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using Xunit;

namespace OrbitAOS.V6.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock = new();

    private HomeController CreateController()
    {
        var controller = new HomeController(_loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        var controller = CreateController();
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var controller = CreateController();
        var result = controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResultWithErrorViewModel()
    {
        var controller = CreateController();
        var result = controller.Error() as ViewResult;
        Assert.NotNull(result);
        var model = Assert.IsType<ErrorViewModel>(result.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public void Error_ModelShowRequestId_FalseWhenRequestIdNull()
    {
        var model = new ErrorViewModel { RequestId = null };
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void Error_ModelShowRequestId_TrueWhenRequestIdSet()
    {
        var model = new ErrorViewModel { RequestId = "test-id" };
        Assert.True(model.ShowRequestId);
    }
}
```

### Integration Tests
```csharp
// Integration/WebApplicationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OrbitAOS.V6.Tests.Integration;

public class WebApplicationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebApplicationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Privacy")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Get_HomeIndex_ReturnsWelcomePage()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Welcome", content);
    }
}
```

### DbContext Tests
```csharp
// Data/ApplicationDbContextTests.cs
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;
using Xunit;

namespace OrbitAOS.V6.Tests.Data;

public class ApplicationDbContextTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void DbContext_CanBeCreated()
    {
        using var context = CreateInMemoryContext();
        Assert.NotNull(context);
    }

    [Fact]
    public async Task DbContext_CanSaveAndRetrieveData()
    {
        using var context = CreateInMemoryContext();
        await context.Database.EnsureCreatedAsync();
        // Add test data and verify retrieval
        Assert.True(await context.Database.CanConnectAsync());
    }
}
```

---

## SECTION 8 — BUILD VERIFICATION STEPS (TEAM MUST RUN AFTER MIGRATION)

> **IMPORTANT:** These steps must be run by the development team AFTER completing the migration. Do NOT run these during the analysis phase.

### Step 1: Verify .NET 10 SDK
```bash
dotnet --version
# Expected: 10.x.x
dotnet --list-sdks
# Expected: 10.x.x [...] listed
```

### Step 2: Create Solution File
```bash
cd /path/to/workspace/DTNTest22
dotnet new sln -n OrbitAOS
dotnet sln OrbitAOS.sln add OrbitAOS.V6/OrbitAOS.V6.csproj
```

### Step 3: Restore Packages
```bash
dotnet restore OrbitAOS.sln
# Expected: Restore completed successfully
```

### Step 4: Build Solution
```bash
dotnet build OrbitAOS.sln --configuration Release
# Expected:
# Build succeeded.
# 0 Error(s)
# 0 Warning(s)
```

### Step 5: Run Tests
```bash
dotnet test OrbitAOS.sln --configuration Release --verbosity normal
# Expected:
# Passed! - Failed: 0, Passed: N, Skipped: 0, Total: N
```

### Step 6: Publish
```bash
dotnet publish OrbitAOS.V6/OrbitAOS.V6.csproj \
    --configuration Release \
    --output ./publish \
    --runtime win-x64 \
    --self-contained false
# Expected: Publish succeeded
```

---

## APPENDIX — COMPLETE ISSUES LIST

| ID | Severity | File | Line | Issue |
|---|---|---|---|---|
| issue-1 | critical | OrbitAOS.V6.csproj | 4 | TargetFramework is net6.0, must be net10.0 |
| issue-2 | critical | OrbitAOS.V6.csproj | 8-12 | All packages pinned to 6.0.19, incompatible with net10.0 |
| issue-3 | high | Data/Migrations/ApplicationDbContextModelSnapshot.cs | 17 | ProductVersion = "3.0.0" — stale snapshot |
| issue-4 | high | Data/Migrations/00000000000000_CreateIdentitySchema.Designer.cs | 17 | ProductVersion = "3.0.0" — stale designer |
| issue-5 | high | Data/Migrations/00000000000000_CreateIdentitySchema.cs | 10-80 | Deprecated nullable: column parameter |
| issue-6 | high | Data/Migrations/ApplicationDbContextModelSnapshot.cs | 43,75 | Deprecated HasName() index API |
| issue-7 | high | Data/Migrations/00000000000000_CreateIdentitySchema.cs | 45,65 | Deprecated SqlServerValueGenerationStrategy annotation |
| issue-8 | high | (workspace root) | N/A | No solution file (.sln) |
| issue-9 | medium | appsettings.json | 3 | LocalDB connection string not production-ready |
| issue-10 | medium | appsettings.json | 3 | MultipleActiveResultSets=true not needed |
| issue-11 | medium | Program.cs | 12 | RequireConfirmedAccount=true without email service |
| issue-12 | medium | Program.cs | 12 | No IEmailSender implementation registered |
| issue-13 | medium | Program.cs | 7 | GetConnectionString() returns nullable string, no null check |
| issue-14 | medium | Views/Shared/_Layout.cshtml | 8 | CSS Isolation file may not generate after TFM upgrade |
| issue-15 | medium | (project) | N/A | No unit test project |
| issue-16 | medium | (project) | N/A | No health check endpoint |
| issue-17 | medium | (project) | N/A | No production HTTPS certificate configuration |
| issue-18 | low | Views/Home/Index.cshtml | 5 | Deprecated docs.microsoft.com URL |
| issue-19 | low | Properties/launchSettings.json | 2-8 | IIS Express profile may not work with .NET 10 |
| issue-20 | low | (project) | N/A | No structured logging for production |
| issue-21 | low | (project) | N/A | No HSTS max-age configuration for production |
| issue-22 | low | (project) | N/A | Client-side libraries (Bootstrap, jQuery) may need version updates |

---

*End of Migration Analysis Report — OrbitAOS.V6*
*Generated: 2025-07-14*
*Analysis ID: mvc-migration-analysis-2025-07-14*
