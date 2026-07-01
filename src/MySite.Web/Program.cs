using Microsoft.AspNetCore.Authentication.Cookies;
using MySite.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "MySite.Admin";
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<ISitePageStore, JsonSitePageStore>();

var app = builder.Build();

await app.Services.GetRequiredService<ISitePageStore>().EnsureSeedDataAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "site-page",
    pattern: "{*slug}",
    defaults: new { controller = "Home", action = "Index" });

app.Run();
