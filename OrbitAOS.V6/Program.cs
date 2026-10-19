// ============================================================
// Program.cs – ASP.NET Core MVC on .NET 10
// Clean Architecture: Web (Presentation) Layer entry point
//
// Replaces:
//   - Global.asax / Global.asax.cs
//   - App_Start/RouteConfig.cs
//   - App_Start/FilterConfig.cs
//   - App_Start/BundleConfig.cs
//   - web.config <system.web> section
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------
// 1. Configuration – replaces web.config <appSettings> and
//    <connectionStrings> sections.  Values come from:
//      appsettings.json → appsettings.{Environment}.json →
//      environment variables → user secrets (Development)
// ------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found in appsettings.json.");

// ------------------------------------------------------------------
// 2. Data Layer – EF Core (replaces Entity Framework 6)
//    ApplicationDbContext lives in OrbitAOS.Infrastructure
// ------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            // Enable resilient connection strategy for transient SQL errors
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// Developer-friendly EF Core exception pages (Development only)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ------------------------------------------------------------------
// 3. ASP.NET Core Identity (replaces ASP.NET Identity 2.x / Forms Auth)
//    - AddDefaultIdentity registers UserManager, SignInManager, etc.
//    - AddRoles<IdentityRole> enables role-based authorization
//    - .AddEntityFrameworkStores wires Identity to EF Core
// ------------------------------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Password policy – adjust to match legacy web.config membership settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Account confirmation – set to false if legacy app did not require it
    options.SignIn.RequireConfirmedAccount = true;

    // Lockout settings (replaces web.config membership lockout)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Cookie authentication settings (replaces Forms Authentication in web.config)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // matches legacy timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ------------------------------------------------------------------
// 4. Session State (replaces System.Web.SessionState)
//    Uses in-memory distributed cache for single-server deployments.
//    For multi-server: swap AddDistributedMemoryCache for
//    AddStackExchangeRedisCache or AddDistributedSqlServerCache.
// ------------------------------------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ------------------------------------------------------------------
// 5. MVC / Razor Pages
//    AddControllersWithViews – registers MVC controllers + Razor views
//    AddRazorPages – required for Identity UI scaffolded Razor Pages
// ------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Global filters (replaces App_Start/FilterConfig.cs)
    // Example: options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddRazorPages();

// ------------------------------------------------------------------
// 6. Response Caching (replaces [OutputCache] attribute)
//    Use [ResponseCache] attribute on actions or controllers.
// ------------------------------------------------------------------
builder.Services.AddResponseCaching();

// ------------------------------------------------------------------
// 7. Application Services – Clean Architecture DI registrations
//    All business logic lives in OrbitAOS.Application and
//    OrbitAOS.Infrastructure; registered here via extension methods.
// ------------------------------------------------------------------
builder.Services.AddApplicationServices();   // defined in OrbitAOS.Application
builder.Services.AddInfrastructureServices(builder.Configuration); // defined in OrbitAOS.Infrastructure

// ------------------------------------------------------------------
// 8. HTTP Client Factory (replaces WebClient / HttpWebRequest)
// ------------------------------------------------------------------
builder.Services.AddHttpClient();

// ------------------------------------------------------------------
// 9. Logging – structured logging via built-in ILogger<T>
//    Replaces log4net / NLog web.config configuration.
//    Configure sinks in appsettings.json "Logging" section.
// ------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ------------------------------------------------------------------
// Build the application
// ------------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------------
// 10. Middleware Pipeline (replaces HTTP Modules in web.config)
//     Order matters – follow ASP.NET Core recommended order.
// ------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Shows detailed EF Core migration errors in browser
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    // Production error handler (replaces customErrors in web.config)
    app.UseExceptionHandler("/Home/Error");

    // HTTP Strict Transport Security (HSTS)
    // Default 30-day max-age; adjust for production
    app.UseHsts();
}

// Redirect HTTP → HTTPS (replaces IIS URL Rewrite / web.config httpsRedirect)
app.UseHttpsRedirection();

// Serve static files from wwwroot/ (replaces /Content, /Scripts, /Images)
app.UseStaticFiles();

// Response caching middleware (must come before routing)
app.UseResponseCaching();

// Routing middleware
app.UseRouting();

// Session middleware (must come after UseRouting, before UseAuthorization)
app.UseSession();

// Authentication middleware (replaces FormsAuthentication module)
app.UseAuthentication();

// Authorization middleware
app.UseAuthorization();

// ------------------------------------------------------------------
// 11. Route Configuration (replaces App_Start/RouteConfig.cs)
//     Conventional MVC route: {controller}/{action}/{id?}
//     Attribute routing is also supported via [Route] attributes.
// ------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages (required for Identity UI)
app.MapRazorPages();

app.Run();
