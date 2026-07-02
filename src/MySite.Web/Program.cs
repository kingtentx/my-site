using CIMC.EntityFramework;
using CIMC.EntityFramework.Repositories;
using CIMC.WebSite.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                       ?? string.Empty;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ISiteBuilderService, SiteBuilderService>();
builder.Services.AddScoped<IConfigurationLike, ConfigurationAdapter>();

var corsOrigins = (builder.Configuration["App:CorsOrigins"] ?? string.Empty)
    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else
        {
            policy.AllowAnyHeader().AllowAnyMethod();
        }
    });
});

if (builder.Configuration.GetValue<bool>("Redis:IsEnabled"))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["Redis:Configuration"];
        options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "WebSite";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "MySite.Admin";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Default or MYSQL_CONNECTION is required.");
}

using (var scope = app.Services.CreateScope())
{
    var seeder = new DataSeeder(scope.ServiceProvider.GetRequiredService<AppDbContext>(), scope.ServiceProvider.GetRequiredService<IConfigurationLike>());
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLogMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "site-page",
    pattern: "{*slug}",
    defaults: new { controller = "Home", action = "Index" });

app.Run();
