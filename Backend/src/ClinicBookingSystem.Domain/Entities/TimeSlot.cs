namespace ClinicBookingSystem.Domain.Entities;

public class TimeSlot
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public string? BlockReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Doctor? Doctor { get; set; }

    public int GetDurationInMinutes() => (int)(EndTime - StartTime).TotalMinutes;
    
    public bool IsValidTimeSlot() => EndTime > StartTime && EndTime.Subtract(StartTime).TotalMinutes >= 15;
    
    public bool IsExpired() => EndTime < DateTime.UtcNow;
    
    public bool IsCurrentlyActive() => StartTime <= DateTime.UtcNow && DateTime.UtcNow <= EndTime;
    
    public bool OverlapsWith(DateTime otherStart, DateTime otherEnd)
    {
        return StartTime < otherEnd && EndTime > otherStart;
    }
    
    public bool CanAccommodateDuration(int durationInMinutes)
    {
        return GetDurationInMinutes() >= durationInMinutes;
    }
}
