using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Add services to the container
// ============================================================

// 1. Add DbContext with SQL Server
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Add OpenAPI (native .NET 10)
builder.Services.AddOpenApi();

// 4. Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

// 5. Add CORS (for Part 3 MVC app to call this API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Register Password Service (for hashing passwords)
builder.Services.AddScoped<IPasswordService, PasswordService>();

// ============================================================
// Configure the HTTP request pipeline
// ============================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // Serves /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();