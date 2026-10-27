using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// cr-dotnet-1016: ResponseCompression with Brotli and Gzip for AWS-hosted app
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/html",
        "text/css",
        "application/javascript",
        "application/json",
        "image/svg+xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// cr-dotnet-1011: WebOptimizer for bundling and minifying CSS/JS
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.min.css",
        "css/bootstrap.min.css",
        "css/site.css");

    pipeline.AddJavaScriptBundle("/js/bundle.min.js",
        "js/jquery.min.js",
        "js/bootstrap.bundle.min.js",
        "js/site.js");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

// cr-dotnet-1011: WebOptimizer must be before UseStaticFiles
app.UseWebOptimizer();

// cr-dotnet-1010: StaticFileMiddleware with Cache-Control headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache fingerprinted assets for 1 year (immutable)
        if (ctx.Context.Request.Path.Value!.Contains("-v") ||
            ctx.Context.Request.Query.ContainsKey("v"))
        {
            ctx.Context.Response.Headers["Cache-Control"] =
                "public, max-age=31536000, immutable";
        }
        else
        {
            // Non-fingerprinted assets: revalidate after 1 hour
            ctx.Context.Response.Headers["Cache-Control"] =
                "public, max-age=3600, must-revalidate";
        }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
