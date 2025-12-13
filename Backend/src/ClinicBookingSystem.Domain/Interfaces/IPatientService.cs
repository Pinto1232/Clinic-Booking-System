using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IPatientService
{
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientByEmailAsync(string email);
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<Patient> RegisterPatientAsync(string firstName, string lastName, string email, string? phoneNumber);
    Task<Patient> UpdatePatientAsync(int id, string firstName, string lastName, string email, string? phoneNumber);
    Task<bool> DeletePatientAsync(int id);
}
