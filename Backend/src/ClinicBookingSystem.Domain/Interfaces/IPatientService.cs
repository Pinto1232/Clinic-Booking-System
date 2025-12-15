using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IPatientService
{
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientByEmailAsync(string email);
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
    Task<Patient> RegisterPatientAsync(string firstName, string lastName, string email, string? phoneNumber);
    Task<Patient> UpdatePatientAsync(int id, string firstName, string lastName, string email, string? phoneNumber);
    Task<Patient> UpdatePatientProfileAsync(
        int id,
        string firstName,
        string lastName,
        string? phoneNumber,
        DateTime? dateOfBirth,
        Gender gender,
        string? address,
        string? city,
        string? state,
        string? zipCode,
        string? insuranceProvider,
        string? insurancePolicyNumber,
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? emergencyContactRelationship,
        string? bloodType,
        string? allergies,
        string? medicalNotes);
    Task<bool> DeletePatientAsync(int id);
}
