using System;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octopus.Api.Data;

namespace Octopus.Api.Tests.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Replaces the production SQLite database with an in-memory SQLite database,
/// clears any demo data seeded by Program.cs, and seeds a minimal test dataset.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Keep the in-memory SQLite connection open for the lifetime of the factory.
        // SQLite in-memory databases are destroyed when the connection closes.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration (Sqlite with file-based DB)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Register DbContext backed by the shared in-memory SQLite connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Program.cs seeds demo data via SeedData.Initialize — clear it and
        // replace with the minimal test dataset from SeedHelper.
        SeedDatabase(host.Services);

        return host;
    }

    private static void SeedDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Wipe everything Program.cs may have seeded
        db.Assignments.RemoveRange(db.Assignments);
        db.Ships.RemoveRange(db.Ships);
        db.Docks.RemoveRange(db.Docks);
        db.TerminalStates.RemoveRange(db.TerminalStates);
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();

        // Seed the minimal test dataset (3 docks, 1 terminal state)
        SeedHelper.SeedBasicData(db);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
