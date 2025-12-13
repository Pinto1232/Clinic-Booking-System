using ClinicBookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Infrastructure.Data;

public class ClinicBookingSystemDbContext : DbContext
{
    public ClinicBookingSystemDbContext(DbContextOptions<ClinicBookingSystemDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Patient entity
        modelBuilder.Entity<Patient>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Patient>()
            .Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Patient>()
            .Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Patient>()
            .Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Patient>()
            .Property(p => p.PhoneNumber)
            .HasMaxLength(20);
    }
}
