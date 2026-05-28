using Microsoft.EntityFrameworkCore;
using MyPetClinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyPetClinic.Infrastructure;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MemoryCache for OTP storage
builder.Services.AddMemoryCache();

// Add Infrastructure Services (Clean Architecture)
builder.Services.AddInfrastructureServices();

// Add Cookie Authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        // You can also add options.CallbackPath = "/signin-google"; if you want to explicitly map it, but it's the default.
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/fix-role", async (MyPetClinic.Infrastructure.Persistence.ApplicationDbContext db) => {
    var doctorRole = db.Roles.FirstOrDefault(r => r.Name == "doctor");
    var user = db.Users.FirstOrDefault(u => u.Email == "doctor@mypetclinic.com");
    if (user != null && doctorRole != null) {
        user.RoleId = doctorRole.Id;
        await db.SaveChangesAsync();
        return $"OK - Role set to {doctorRole.Name} (Id: {doctorRole.Id}) for user {user.Email}";
    }
    return $"Error: User (found: {user != null}) or Role (found: {doctorRole != null}) not found";
});

app.Run();
