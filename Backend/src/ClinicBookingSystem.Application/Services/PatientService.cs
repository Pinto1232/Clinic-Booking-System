using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;

namespace ClinicBookingSystem.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(id));

        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<Patient?> GetPatientByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _patientRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    public async Task<Patient> RegisterPatientAsync(string firstName, string lastName, string email, string? phoneNumber)
    {
        ValidatePatientInput(firstName, lastName, email);

        var existingPatient = await _patientRepository.GetByEmailAsync(email);
        if (existingPatient != null)
            throw new InvalidOperationException($"A patient with email {email} already exists");

        var patient = new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        return await _patientRepository.AddAsync(patient);
    }

    public async Task<Patient> UpdatePatientAsync(int id, string firstName, string lastName, string email, string? phoneNumber)
    {
        if (id <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(id));

        ValidatePatientInput(firstName, lastName, email);

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            throw new KeyNotFoundException($"Patient with ID {id} not found");

        // Check if email is already taken by another patient
        var existingPatient = await _patientRepository.GetByEmailAsync(email);
        if (existingPatient != null && existingPatient.Id != id)
            throw new InvalidOperationException($"Email {email} is already in use");

        patient.FirstName = firstName;
        patient.LastName = lastName;
        patient.Email = email;
        patient.PhoneNumber = phoneNumber;

        return await _patientRepository.UpdateAsync(patient);
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(id));

        return await _patientRepository.DeleteAsync(id);
    }

    private static void ValidatePatientInput(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid", nameof(email));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
