using Octopus.Api.Data;
using Octopus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddSingleton<ShipService>();
builder.Services.AddSingleton<BerthService>();
builder.Services.AddSingleton<AssignmentService>();
builder.Services.AddSingleton<SystemService>();

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

var db = app.Services.GetRequiredService<AppDbContext>();
SeedData.Initialize(db);


app.UseCors("OctopusUi");
app.MapControllers();

app.Run();
