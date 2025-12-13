using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;

namespace ClinicBookingSystem.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(id));

        return await _doctorRepository.GetByIdAsync(id);
    }

    public async Task<Doctor?> GetDoctorByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _doctorRepository.GetByEmailAsync(email);
    }

    public async Task<Doctor?> GetDoctorByLicenseNumberAsync(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number cannot be empty", nameof(licenseNumber));

        return await _doctorRepository.GetByLicenseNumberAsync(licenseNumber);
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        return await _doctorRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization cannot be empty", nameof(specialization));

        return await _doctorRepository.GetBySpecializationAsync(specialization);
    }

    public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
    {
        return await _doctorRepository.GetAvailableDoctorsAsync();
    }

    public async Task<Doctor> RegisterDoctorAsync(string firstName, string lastName, string email, string? phoneNumber, string specialization, string? licenseNumber)
    {
        ValidateDoctorInput(firstName, lastName, email, specialization);

        // Check if email already exists
        var existingDoctor = await _doctorRepository.GetByEmailAsync(email);
        if (existingDoctor != null)
            throw new InvalidOperationException($"Doctor with email {email} already exists");

        // Check if license number already exists (if provided)
        if (!string.IsNullOrWhiteSpace(licenseNumber))
        {
            var doctorWithLicense = await _doctorRepository.GetByLicenseNumberAsync(licenseNumber);
            if (doctorWithLicense != null)
                throw new InvalidOperationException($"Doctor with license number {licenseNumber} already exists");
        }

        var doctor = new Doctor
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLower(),
            PhoneNumber = phoneNumber?.Trim(),
            Specialization = specialization.Trim(),
            LicenseNumber = licenseNumber?.Trim(),
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _doctorRepository.AddAsync(doctor);
    }

    public async Task<Doctor> UpdateDoctorAsync(int id, string firstName, string lastName, string email, string? phoneNumber, string specialization, string? licenseNumber, bool isAvailable)
    {
        if (id <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(id));

        ValidateDoctorInput(firstName, lastName, email, specialization);

        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {id} not found");

        // Check if new email already exists (if different)
        if (doctor.Email != email.ToLower())
        {
            var existingDoctor = await _doctorRepository.GetByEmailAsync(email);
            if (existingDoctor != null)
                throw new InvalidOperationException($"Doctor with email {email} already exists");
        }

        doctor.FirstName = firstName.Trim();
        doctor.LastName = lastName.Trim();
        doctor.Email = email.Trim().ToLower();
        doctor.PhoneNumber = phoneNumber?.Trim();
        doctor.Specialization = specialization.Trim();
        doctor.LicenseNumber = licenseNumber?.Trim();
        doctor.IsAvailable = isAvailable;
        doctor.UpdatedAt = DateTime.UtcNow;

        return await _doctorRepository.UpdateAsync(doctor);
    }

    public async Task<bool> DeleteDoctorAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(id));

        return await _doctorRepository.DeleteAsync(id);
    }

    public async Task<bool> SetDoctorAvailabilityAsync(int id, bool isAvailable)
    {
        if (id <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(id));

        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {id} not found");

        doctor.IsAvailable = isAvailable;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _doctorRepository.UpdateAsync(doctor);
        return true;
    }

    private static void ValidateDoctorInput(string firstName, string lastName, string email, string specialization)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization cannot be empty", nameof(specialization));
    }
}
