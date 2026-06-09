using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Middleware;
using Octopus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("OctopusDb")));

// Services (Scoped — matches DbContext lifetime)
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<IBerthService, BerthService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddSingleton<ISystemService, SystemService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OctopusUi", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure DB directory exists for SQLite
var dbPath = builder.Configuration.GetConnectionString("OctopusDb");
if (dbPath is not null)
{
    var dir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
    if (dir is not null && !Directory.Exists(dir))
        Directory.CreateDirectory(dir);
}

// Seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("OctopusUi");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
