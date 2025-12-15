namespace ClinicBookingSystem.Frontend.Models;

public enum Gender
{
    NotSpecified = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public class PatientProfileModel
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
    public Gender Gender { get; set; } = Gender.NotSpecified;
    
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
    
    public string GenderDisplay => Gender switch
    {
        Gender.Male => "Male",
        Gender.Female => "Female",
        Gender.Other => "Other",
        _ => "Not Specified"
    };
}

public class PatientSearchResult
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdatePatientProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalNotes { get; set; }
}

public class DoctorModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    public string FullName => $"Dr. {FirstName} {LastName}";
    public string DisplayName => $"{FullName} - {Specialization}";
}

public class TimeSlotModel
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsBlocked { get; set; } = false;

    public string DisplayTime => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
}

public class AppointmentModel
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; } = 30;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime AppointmentDateTime => AppointmentDate.Date.Add(AppointmentTime.TimeOfDay);
    public string DisplayDateTime => $"{AppointmentDate:MMM dd, yyyy} at {AppointmentTime:HH:mm}";
    
    public string StatusDisplay => Status switch
    {
        AppointmentStatus.Scheduled => "Scheduled",
        AppointmentStatus.Confirmed => "Confirmed",
        AppointmentStatus.InProgress => "In Progress",
        AppointmentStatus.Completed => "Completed",
        AppointmentStatus.Cancelled => "Cancelled",
        AppointmentStatus.NoShow => "No Show",
        _ => "Unknown"
    };

    public MudBlazor.Color StatusColor => Status switch
    {
        AppointmentStatus.Scheduled => MudBlazor.Color.Info,
        AppointmentStatus.Confirmed => MudBlazor.Color.Success,
        AppointmentStatus.InProgress => MudBlazor.Color.Warning,
        AppointmentStatus.Completed => MudBlazor.Color.Default,
        AppointmentStatus.Cancelled => MudBlazor.Color.Error,
        AppointmentStatus.NoShow => MudBlazor.Color.Dark,
        _ => MudBlazor.Color.Default
    };
}

public enum AppointmentStatus
{
    Scheduled = 0,
    Confirmed = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public class CreateAppointmentRequest
{
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; } = 30;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
