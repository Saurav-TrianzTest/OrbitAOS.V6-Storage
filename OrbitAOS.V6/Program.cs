using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Data / EF Core configuration
// Upgraded from EF Core 6 to EF Core 8 (Microsoft.EntityFrameworkCore.SqlServer 8.0.0)
// Null guard added: throws InvalidOperationException if connection string is missing
// -----------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Developer-friendly EF Core exception pages (development only)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -----------------------------------------------------------------------
// ASP.NET Core Identity configuration
// Upgraded from Identity 6.0.x to Identity 8.0.x
// RequireConfirmedAccount = true enforces e-mail confirmation before sign-in
// AddRoles<IdentityRole>() enables role-based authorization support
// -----------------------------------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    // .NET 8 recommended password policy (adjust to match your security requirements)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// -----------------------------------------------------------------------
// Data Protection key persistence
// Keys are persisted to the file system so that authentication cookies
// remain valid across application restarts in production.
// For cloud/containerized deployments, consider PersistKeysToDbContext<T>()
// or PersistKeysToAzureBlobStorage() instead.
// -----------------------------------------------------------------------
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("OrbitAOS.V6");

// -----------------------------------------------------------------------
// Email sender
// A no-op implementation is registered for development/testing.
// Replace NoOpEmailSender with a real SMTP/SendGrid/etc. implementation
// before deploying to production.
// -----------------------------------------------------------------------
builder.Services.AddTransient<IEmailSender,
    Microsoft.AspNetCore.Identity.UI.Services.NoOpEmailSender>();

// -----------------------------------------------------------------------
// Health checks
// Adds a /health endpoint that reports the application and database status.
// Useful for load balancers, Kubernetes liveness/readiness probes, and
// Azure App Service health monitoring.
// -----------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// -----------------------------------------------------------------------
// MVC services
// AddControllersWithViews registers MVC controllers + Razor views
// -----------------------------------------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------------------------------------
// Build the application
// -----------------------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------------------
// HTTP request pipeline
// -----------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Shows detailed EF Core migration errors in the browser during development
    app.UseMigrationsEndPoint();
}
else
{
    // Production error handler – redirects to /Home/Error
    app.UseExceptionHandler("/Home/Error");

    // HTTP Strict Transport Security (HSTS)
    // Default value is 30 days; adjust for production as needed.
    // See: https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

// Redirect HTTP → HTTPS
app.UseHttpsRedirection();

// Serve static files from wwwroot (CSS, JS, images, etc.)
app.UseStaticFiles();

// Enable endpoint routing
app.UseRouting();

// Authentication middleware must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------
// Route configuration
// Default MVC route: /{controller=Home}/{action=Index}/{id?}
// MapRazorPages is required for the scaffolded Identity Razor Pages (Areas/Identity)
// MapHealthChecks exposes the /health endpoint for production monitoring
// -----------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Health check endpoint – returns 200 OK when the app and DB are healthy
app.MapHealthChecks("/health");

app.Run();
