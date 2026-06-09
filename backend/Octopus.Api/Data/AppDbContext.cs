using Microsoft.EntityFrameworkCore;
using Octopus.Api.Models;

namespace Octopus.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Berth> Berths => Set<Berth>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.Property(s => s.ImoNumber).HasMaxLength(20).IsRequired();
            entity.Property(s => s.CargoType).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Status).HasMaxLength(50).HasDefaultValue("Waiting");
        });

        modelBuilder.Entity<Berth>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).HasMaxLength(200).IsRequired();
            entity.Property(b => b.MaxDraftMeters).HasPrecision(6, 2);
            entity.Property(b => b.IsAvailable).HasDefaultValue(true);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Status).HasMaxLength(50).HasDefaultValue("Planned");

            entity.HasOne<Ship>()
                .WithMany()
                .HasForeignKey(a => a.ShipId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Berth>()
                .WithMany()
                .HasForeignKey(a => a.BerthId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
