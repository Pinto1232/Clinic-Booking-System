namespace ClinicBookingSystem.Domain.Entities;

public enum Gender
{
    NotSpecified = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    
    // Extended profile fields
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    
    // Insurance information
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    // Medical information
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalNotes { get; set; }
    
    // Profile status
    public bool IsProfileComplete { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";
    
    public int? GetAge()
    {
        if (!DateOfBirth.HasValue) return null;
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
        return age;
    }
}
