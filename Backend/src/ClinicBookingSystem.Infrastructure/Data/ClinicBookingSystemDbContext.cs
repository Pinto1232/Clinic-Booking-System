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
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<User> Users => Set<User>();

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

        // Configure Doctor entity
        modelBuilder.Entity<Doctor>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<Doctor>()
            .Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Doctor>()
            .Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Doctor>()
            .Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Doctor>()
            .Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure TimeSlot entity
        modelBuilder.Entity<TimeSlot>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<TimeSlot>()
            .HasOne(t => t.Doctor)
            .WithMany()
            .HasForeignKey(t => t.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure User entity
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Patient)
            .WithMany()
            .HasForeignKey(u => u.PatientId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Doctor)
            .WithMany()
            .HasForeignKey(u => u.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
