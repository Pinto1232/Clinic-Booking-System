namespace ClinicBookingSystem.API.DTOs;

public class CreateTimeSlotRequest
{
    public int DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class CreateBulkTimeSlotsRequest
{
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationInMinutes { get; set; } = 30;
}

public class UpdateTimeSlotRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
}

public class BlockTimeSlotRequest
{
    public string? BlockReason { get; set; }
}

public class TimeSlotResponse
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationInMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public bool IsExpired { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
