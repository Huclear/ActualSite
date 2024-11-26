using ActualSite;
using ActualSite.Data;
using ActualSite.Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
TokenConfig conf = new TokenConfig(builder.Configuration["JWT:issuer"], builder.Configuration["JWT:audience"], (long)1000 * 60 * 60 * 24 * 30, builder.Configuration["JWT:secret"]);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DishesContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddSingleton(conf);
builder.Services.AddSingleton<IHashingService>(opts => new SHA256HashingService());
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "Admin");
    });
});
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(opts =>
    {
        opts.LoginPath = "/Auth/SignIn/";
    })
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "static")),
    RequestPath = "/static"
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}");

app.Run();
