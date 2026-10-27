using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Data / EF Core configuration
// Upgraded from EF Core 6 to EF Core 8 (Microsoft.EntityFrameworkCore.SqlServer 8.0.0)
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
.AddEntityFrameworkStores<ApplicationDbContext>();

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
// -----------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
