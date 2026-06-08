using Octopus.Api.Data;
using Octopus.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("OctopusDb"));
});

builder.Services.AddScoped<ShipService>();
builder.Services.AddScoped<BerthService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<SystemService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("OctopusUi");
app.MapControllers();

app.Run();
