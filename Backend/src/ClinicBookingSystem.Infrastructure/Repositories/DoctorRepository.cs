using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using ClinicBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicBookingSystemDbContext _context;

    public DoctorRepository(ClinicBookingSystemDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Doctor?> GetByEmailAsync(string email)
    {
        return await _context.Doctors.FirstOrDefaultAsync(d => d.Email == email);
    }

    public async Task<Doctor?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.Doctors.FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber);
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _context.Doctors.ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization)
    {
        return await _context.Doctors
            .Where(d => d.Specialization.ToLower().Contains(specialization.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
    {
        return await _context.Doctors
            .Where(d => d.IsAvailable)
            .ToListAsync();
    }

    public async Task<Doctor> AddAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        doctor.UpdatedAt = DateTime.UtcNow;
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await GetByIdAsync(id);
        if (doctor == null)
            return false;

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Doctors.AnyAsync(d => d.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Doctors.AnyAsync(d => d.Email == email);
    }
}
