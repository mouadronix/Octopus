using Octopus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Octopus.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Berth> Berths => Set<Berth>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SystemState> SystemStates => Set<SystemState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ship>()
            .Property(ship => ship.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Ship>()
            .Property(ship => ship.Size)
            .HasConversion<string>();

        modelBuilder.Entity<Berth>()
            .Property(berth => berth.Size)
            .HasConversion<string>();

        modelBuilder.Entity<Assignment>()
            .HasOne(assignment => assignment.Ship)
            .WithMany(ship => ship.Assignments)
            .HasForeignKey(assignment => assignment.ShipId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(assignment => assignment.Berth)
            .WithMany(berth => berth.Assignments)
            .HasForeignKey(assignment => assignment.BerthId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
