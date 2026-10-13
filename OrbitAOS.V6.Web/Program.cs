using Microsoft.AspNetCore.Identity;
using OrbitAOS.V6.Infrastructure.Data;
using OrbitAOS.V6.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string from appsettings.json
// Replaces legacy web.config connectionStrings section
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register Infrastructure services (DbContext, Repositories, Application Services)
// Replaces legacy Global.asax Application_Start and App_Start/FilterConfig.cs
builder.Services.AddInfrastructureServices(connectionString);

// Configure ASP.NET Core Identity
// Migrated from ASP.NET Identity 2.x to ASP.NET Core Identity 8
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC with Views - replaces legacy System.Web.Mvc registration
builder.Services.AddControllersWithViews();

// Add session support - replaces legacy System.Web.SessionState
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add response caching - replaces legacy OutputCache attribute
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline
// Replaces legacy Global.asax Application_BeginRequest and HTTP Modules
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces legacy Application_Error in Global.asax
    app.UseExceptionHandler("/Home/Error");
    // HSTS - not available in legacy ASP.NET MVC
    app.UseHsts();
}

// Replaces legacy HTTP redirect module
app.UseHttpsRedirection();

// Serves static files from wwwroot - replaces legacy /Content and /Scripts folders
app.UseStaticFiles();

// Routing middleware - replaces legacy RouteConfig.cs in App_Start
app.UseRouting();

// Session middleware - replaces legacy System.Web.SessionState
app.UseSession();

// Response caching middleware
app.UseResponseCaching();

// Authentication middleware - replaces legacy FormsAuthentication module
app.UseAuthentication();

// Authorization middleware
app.UseAuthorization();

// Map controller routes - replaces legacy RouteConfig.RegisterRoutes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for Identity UI
app.MapRazorPages();

app.Run();
