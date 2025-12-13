namespace ClinicBookingSystem.Domain.Entities;

public enum AppointmentStatus
{
    Scheduled = 0,
    Confirmed = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; } = 30;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation properties
    public virtual Patient? Patient { get; set; }
    public virtual Doctor? Doctor { get; set; }

    public DateTime GetAppointmentDateTime() => AppointmentDate.Add(AppointmentTime.TimeOfDay);
    
    public DateTime GetEndTime() => GetAppointmentDateTime().AddMinutes(DurationInMinutes);
    
    public bool IsUpcoming() => GetAppointmentDateTime() > DateTime.UtcNow && Status != AppointmentStatus.Cancelled;
    
    public bool IsPast() => GetAppointmentDateTime() < DateTime.UtcNow;
    
    public string GetStatusDisplay() => Status switch
    {
        AppointmentStatus.Scheduled => "Scheduled",
        AppointmentStatus.Confirmed => "Confirmed",
        AppointmentStatus.InProgress => "In Progress",
        AppointmentStatus.Completed => "Completed",
        AppointmentStatus.Cancelled => "Cancelled",
        AppointmentStatus.NoShow => "No Show",
        _ => "Unknown"
    };
}
