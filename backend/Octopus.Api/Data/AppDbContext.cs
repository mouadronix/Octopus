using Microsoft.EntityFrameworkCore;
using Octopus.Api.Models;

namespace Octopus.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Dock> Docks => Set<Dock>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<TerminalState> TerminalStates => Set<TerminalState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ship → Assignment (one-to-one)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Ship)
            .WithOne(s => s.Assignment)
            .HasForeignKey<Assignment>(a => a.ShipId);

        // Dock → Assignments (one-to-many)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Dock)
            .WithMany(d => d.Assignments)
            .HasForeignKey(a => a.DockId);

        // Store enums as strings for readability
        modelBuilder.Entity<Ship>()
            .Property(s => s.Size)
            .HasConversion<string>();

        modelBuilder.Entity<Ship>()
            .Property(s => s.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Dock>()
            .Property(d => d.Size)
            .HasConversion<string>();
    }
}
