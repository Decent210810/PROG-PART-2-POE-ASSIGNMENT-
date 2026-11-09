using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Add OpenAPI
builder.Services.AddOpenApi();

// 4. Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
    // These allow the cookie to work cross-origin
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// 5. Add CORS (Crucial for passing the cookie to the MVC app)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Set-Cookie"); // Exposes the cookie header to MVC
    });
});

// 6. Register Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// ============================================================
// BUILD THE APP
// ============================================================
var app = builder.Build();

// 7. Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RaceDayDbContext>();
        var passwordService = services.GetRequiredService<IPasswordService>();
        await RaceDay.API.Data.DbSeeder.SeedAsync(context, passwordService);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 8. Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC"); // CORS before Session
app.UseSession();        // Session before Auth
app.UseAuthorization();
app.MapControllers();

app.Run();