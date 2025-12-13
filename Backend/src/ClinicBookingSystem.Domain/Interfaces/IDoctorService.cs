using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IDoctorService
{
    Task<Doctor?> GetDoctorByIdAsync(int id);
    Task<Doctor?> GetDoctorByEmailAsync(string email);
    Task<Doctor?> GetDoctorByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
    Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(string specialization);
    Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
    Task<Doctor> RegisterDoctorAsync(string firstName, string lastName, string email, string? phoneNumber, string specialization, string? licenseNumber);
    Task<Doctor> UpdateDoctorAsync(int id, string firstName, string lastName, string email, string? phoneNumber, string specialization, string? licenseNumber, bool isAvailable);
    Task<bool> DeleteDoctorAsync(int id);
    Task<bool> SetDoctorAvailabilityAsync(int id, bool isAvailable);
}
