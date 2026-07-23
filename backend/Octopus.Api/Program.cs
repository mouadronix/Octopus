using Microsoft.EntityFrameworkCore;
using Octopus.Api.Common;
using Octopus.Api.Data;
using Octopus.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Octopus – Port Terminal API",
        Version = "v1",
        Description = "REST API for managing ships, docks (berths), and berth assignments in a port terminal simulation.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Octopus Team"
        }
    });

    // Include XML doc comments from the generated XML file
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var connectionString = builder.Configuration.GetConnectionString("OctopusDb")
    ?? "Data Source=blueharbor.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IAssignmentRepository, EfAssignmentRepository>();
builder.Services.AddScoped<ShipService>();
builder.Services.AddScoped<DockService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<SystemService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Auto-migrate and seed only in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(context);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program { }
