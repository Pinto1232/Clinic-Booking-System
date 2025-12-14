using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using ClinicBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Infrastructure.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly ClinicBookingSystemDbContext _context;

    public TimeSlotRepository(ClinicBookingSystemDbContext context)
    {
        _context = context;
    }

    public async Task<TimeSlot?> GetByIdAsync(int id)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TimeSlot>> GetAllAsync()
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAsync(int doctorId)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId && t.IsAvailable && !t.IsBlocked && t.StartTime > DateTime.UtcNow)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetByDateAsync(DateTime date)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.StartTime.Date == date.Date)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId && t.StartTime.Date == date.Date)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.StartTime.Date >= startDate.Date && t.StartTime.Date <= endDate.Date)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId && 
                       t.StartTime.Date >= startDate.Date && 
                       t.StartTime.Date <= endDate.Date)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId && 
                       t.StartTime.Date == date.Date && 
                       t.IsAvailable && 
                       !t.IsBlocked &&
                       t.StartTime > DateTime.UtcNow)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .Where(t => t.DoctorId == doctorId && 
                       t.StartTime.Date >= startDate.Date && 
                       t.StartTime.Date <= endDate.Date &&
                       t.IsAvailable && 
                       !t.IsBlocked &&
                       t.StartTime > DateTime.UtcNow)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<TimeSlot?> GetByDoctorAndTimeAsync(int doctorId, DateTime startTime, DateTime endTime)
    {
        return await _context.TimeSlots
            .Include(t => t.Doctor)
            .FirstOrDefaultAsync(t => t.DoctorId == doctorId && 
                                      t.StartTime == startTime && 
                                      t.EndTime == endTime);
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateTime startTime, DateTime endTime, int? excludeSlotId = null)
    {
        var query = _context.TimeSlots
            .Where(t => t.DoctorId == doctorId);

        if (excludeSlotId.HasValue)
        {
            query = query.Where(t => t.Id != excludeSlotId.Value);
        }

        return await query.AnyAsync(t => 
            (startTime >= t.StartTime && startTime < t.EndTime) ||
            (endTime > t.StartTime && endTime <= t.EndTime) ||
            (startTime <= t.StartTime && endTime >= t.EndTime));
    }

    public async Task<TimeSlot> AddAsync(TimeSlot timeSlot)
    {
        _context.TimeSlots.Add(timeSlot);
        await _context.SaveChangesAsync();
        return timeSlot;
    }

    public async Task<IEnumerable<TimeSlot>> AddBulkAsync(IEnumerable<TimeSlot> timeSlots)
    {
        var slotsList = timeSlots.ToList();
        _context.TimeSlots.AddRange(slotsList);
        await _context.SaveChangesAsync();
        return slotsList;
    }

    public async Task<TimeSlot> UpdateAsync(TimeSlot timeSlot)
    {
        timeSlot.UpdatedAt = DateTime.UtcNow;
        _context.TimeSlots.Update(timeSlot);
        await _context.SaveChangesAsync();
        return timeSlot;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var timeSlot = await _context.TimeSlots.FindAsync(id);
        if (timeSlot == null)
            return false;

        _context.TimeSlots.Remove(timeSlot);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        var slots = await _context.TimeSlots
            .Where(t => t.DoctorId == doctorId && t.StartTime.Date == date.Date)
            .ToListAsync();

        if (!slots.Any())
            return 0;

        _context.TimeSlots.RemoveRange(slots);
        await _context.SaveChangesAsync();
        return slots.Count;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TimeSlots.AnyAsync(t => t.Id == id);
    }
}
