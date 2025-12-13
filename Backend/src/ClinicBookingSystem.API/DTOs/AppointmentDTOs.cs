namespace ClinicBookingSystem.API.DTOs;

public enum AppointmentStatusDto
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
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; } = 30;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; } = 30;
    public AppointmentStatusDto Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class CancelAppointmentRequest
{
    public string? CancellationReason { get; set; }
}

public class AppointmentResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationInMinutes { get; set; }
    public AppointmentStatusDto Status { get; set; }
    public string? StatusDisplay { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsPast { get; set; }
}
