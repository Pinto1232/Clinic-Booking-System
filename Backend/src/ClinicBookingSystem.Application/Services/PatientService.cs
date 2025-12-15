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

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Patient>();

        var allPatients = await _patientRepository.GetAllAsync();
        var lowerTerm = searchTerm.ToLower().Trim();
        
        return allPatients.Where(p =>
            p.FirstName.ToLower().Contains(lowerTerm) ||
            p.LastName.ToLower().Contains(lowerTerm) ||
            p.Email.ToLower().Contains(lowerTerm) ||
            p.GetFullName().ToLower().Contains(lowerTerm));
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
        patient.UpdatedAt = DateTime.UtcNow;

        return await _patientRepository.UpdateAsync(patient);
    }

    public async Task<Patient> UpdatePatientProfileAsync(
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
        string? medicalNotes)
    {
        if (id <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(id));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            throw new KeyNotFoundException($"Patient with ID {id} not found");

        // Update basic info
        patient.FirstName = firstName.Trim();
        patient.LastName = lastName.Trim();
        patient.PhoneNumber = phoneNumber?.Trim();
        
        // Update personal info
        patient.DateOfBirth = dateOfBirth;
        patient.Gender = gender;
        
        // Update address
        patient.Address = address?.Trim();
        patient.City = city?.Trim();
        patient.State = state?.Trim();
        patient.ZipCode = zipCode?.Trim();
        
        // Update insurance
        patient.InsuranceProvider = insuranceProvider?.Trim();
        patient.InsurancePolicyNumber = insurancePolicyNumber?.Trim();
        
        // Update emergency contact
        patient.EmergencyContactName = emergencyContactName?.Trim();
        patient.EmergencyContactPhone = emergencyContactPhone?.Trim();
        patient.EmergencyContactRelationship = emergencyContactRelationship?.Trim();
        
        // Update medical info
        patient.BloodType = bloodType?.Trim();
        patient.Allergies = allergies?.Trim();
        patient.MedicalNotes = medicalNotes?.Trim();
        
        // Check if profile is complete (basic required fields)
        patient.IsProfileComplete = !string.IsNullOrWhiteSpace(patient.FirstName) &&
                                    !string.IsNullOrWhiteSpace(patient.LastName) &&
                                    !string.IsNullOrWhiteSpace(patient.PhoneNumber) &&
                                    patient.DateOfBirth.HasValue;
        
        patient.UpdatedAt = DateTime.UtcNow;

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
