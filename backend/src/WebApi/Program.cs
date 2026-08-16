using Microsoft.EntityFrameworkCore;
using MyPetClinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyPetClinic.Infrastructure;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // Allow reading numbers from strings (e.g., "70" to int) to prevent 400 Bad Request validation errors
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    // Fix: Ensure all DateTime values are serialized with UTC "Z" suffix
    // so the frontend browser knows to convert from UTC to local time (UTC+7)
    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR and its Pusher
builder.Services.AddSignalR();
builder.Services.AddScoped<MyPetClinic.Application.Interfaces.Services.ISignalRPusher, WebApi.Services.SignalRPusher>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MemoryCache for OTP storage
builder.Services.AddMemoryCache();

// Add Infrastructure Services (Clean Architecture)
builder.Services.AddInfrastructureServices();

// Add Cookie Authentication services for SPA
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.\"}");
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"Bạn không có quyền truy cập vào chức năng này.\"}");
        };
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "DUMMY_CLIENT_ID_TO_PREVENT_CRASH";
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "DUMMY_SECRET";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WebApi.Hubs.NotificationHub>("/hubs/notification");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await ApplicationDbSeeder.SeedAsync(context);
        try { await context.Database.ExecuteSqlRawAsync("UPDATE doctor_schedules SET end_time = '20:00:00'"); } catch {}
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

/// <summary>
/// Custom JSON converter that ensures DateTime values are always serialized
/// with "Z" (UTC) suffix. This fixes the +7 hours timezone display bug
/// caused by Npgsql legacy mode returning DateTime with Kind=Unspecified.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (DateTime.TryParse(str, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var dt))
        {
            return dt;
        }
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Treat Unspecified kind (from Npgsql legacy mode) as UTC and append "Z"
        var utcValue = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

/// <summary>Nullable DateTime variant of UtcDateTimeConverter.</summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly UtcDateTimeConverter _inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return _inner.Read(ref reader, typeof(DateTime), options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue) { writer.WriteNullValue(); return; }
        _inner.Write(writer, value.Value, options);
    }
}
