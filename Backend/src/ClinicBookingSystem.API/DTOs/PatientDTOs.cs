namespace ClinicBookingSystem.API.DTOs;

public enum GenderDto
{
    NotSpecified = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public class CreatePatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UpdatePatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UpdatePatientProfileRequest
{
    // Basic info
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    
    // Personal info
    public DateTime? DateOfBirth { get; set; }
    public GenderDto Gender { get; set; } = GenderDto.NotSpecified;
    
    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    
    // Insurance
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    // Medical info
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalNotes { get; set; }
}

public class PatientResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PatientProfileResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    
    // Personal info
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public GenderDto Gender { get; set; }
    
    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    
    // Insurance
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    // Medical info
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalNotes { get; set; }
    
    // Status
    public bool IsProfileComplete { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
