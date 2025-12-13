using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface ITimeSlotService
{
    Task<TimeSlot?> GetTimeSlotByIdAsync(int id);
    Task<IEnumerable<TimeSlot>> GetAllTimeSlotsAsync();
    Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAsync(int doctorId);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAsync(int doctorId);
    Task<IEnumerable<TimeSlot>> GetTimeSlotsByDateAsync(DateTime date);
    Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<TimeSlot>> GetTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<TimeSlot> CreateTimeSlotAsync(int doctorId, DateTime startTime, DateTime endTime);
    Task<IEnumerable<TimeSlot>> CreateBulkTimeSlotsAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int slotDurationInMinutes);
    Task<TimeSlot> UpdateTimeSlotAsync(int id, DateTime startTime, DateTime endTime, bool isAvailable, bool isBlocked, string? blockReason);
    Task<TimeSlot> BlockTimeSlotAsync(int id, string? blockReason);
    Task<TimeSlot> UnblockTimeSlotAsync(int id);
    Task<bool> DeleteTimeSlotAsync(int id);
    Task<int> DeleteTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date);
}
