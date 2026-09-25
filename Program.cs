using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Hosts like Vercel tell the container which port to listen on via PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(ToNpgsqlConnectionString(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// AddIdentity (not AddDefaultIdentity) — this project has no self-service registration;
// the only login surface is the custom /Admin controller. AddDefaultIdentity would also
// map the built-in Identity UI's Register/Login/ForgotPassword Razor Pages, which would
// stay live (if unlinked) at /Identity/Account/*.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.Configure<PayFastSettings>(builder.Configuration.GetSection("PayFast"));
builder.Services.Configure<WhatsAppCloudSettings>(builder.Configuration.GetSection("WhatsAppCloud"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<WhatsAppNotificationService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartStore>();

builder.Services.AddDataProtection()
    .SetApplicationName("Koolstoof")
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin";
    options.AccessDeniedPath = "/Admin";
});

// The hosting platform terminates HTTPS and forwards plain HTTP to us. Without this,
// Request.Scheme/Host would be the internal ones, and absolute URLs we build (PayFast
// return/notify URLs) and HTTPS redirection would be wrong.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Creates/updates the schema on first run in a fresh environment (e.g. a new
    // database) so it doesn't depend on running `dotnet ef database update` by hand.
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

    await AdminAccountSeeder.SeedAsync(scope.ServiceProvider);
    await RestaurantSettingsSeeder.SeedAsync(scope.ServiceProvider);
    await CatalogSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseForwardedHeaders();

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

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Accepts either a normal Npgsql connection string or a postgres:// URL (the form
// Neon and most hosts hand out).
static string ToNpgsqlConnectionString(string value)
{
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
        SslMode = SslMode.Require,
        // Each serverless instance keeps a small pool so many instances can't exhaust the database.
        MaxPoolSize = 5
    }.ConnectionString;
}
