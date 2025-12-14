namespace ClinicBookingSystem.Domain.Entities;

public enum UserRole
{
    Patient = 0,
    Doctor = 1,
    Admin = 2,
    Receptionist = 3
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Patient;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Foreign key to Patient or Doctor if applicable
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }

    // Navigation properties
    public virtual Patient? Patient { get; set; }
    public virtual Doctor? Doctor { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";
}
