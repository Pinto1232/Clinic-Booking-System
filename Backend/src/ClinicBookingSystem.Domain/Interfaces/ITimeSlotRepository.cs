using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(int id);
    Task<IEnumerable<TimeSlot>> GetAllAsync();
    Task<IEnumerable<TimeSlot>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAsync(int doctorId);
    Task<IEnumerable<TimeSlot>> GetByDateAsync(DateTime date);
    Task<IEnumerable<TimeSlot>> GetByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<TimeSlot>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeSlot>> GetByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<TimeSlot?> GetByDoctorAndTimeAsync(int doctorId, DateTime startTime, DateTime endTime);
    Task<bool> HasConflictAsync(int doctorId, DateTime startTime, DateTime endTime, int? excludeSlotId = null);
    Task<TimeSlot> AddAsync(TimeSlot timeSlot);
    Task<IEnumerable<TimeSlot>> AddBulkAsync(IEnumerable<TimeSlot> timeSlots);
    Task<TimeSlot> UpdateAsync(TimeSlot timeSlot);
    Task<bool> DeleteAsync(int id);
    Task<int> DeleteByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<bool> ExistsAsync(int id);
}
