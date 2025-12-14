using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;

namespace ClinicBookingSystem.Application.Services;

public class TimeSlotService : ITimeSlotService
{
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public TimeSlotService(ITimeSlotRepository timeSlotRepository, IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository)
    {
        _timeSlotRepository = timeSlotRepository;
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<TimeSlot?> GetTimeSlotByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("TimeSlot ID must be greater than 0", nameof(id));

        return await _timeSlotRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TimeSlot>> GetAllTimeSlotsAsync()
    {
        return await _timeSlotRepository.GetAllAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.GetByDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.GetAvailableSlotsByDoctorAsync(doctorId);
    }

    public async Task<IEnumerable<TimeSlot>> GetTimeSlotsByDateAsync(DateTime date)
    {
        if (date.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Date cannot be in the past");

        return await _timeSlotRepository.GetByDateAsync(date.Date);
    }

    public async Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (date.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Date cannot be in the past");

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.GetByDoctorAndDateAsync(doctorId, date.Date);
    }

    public async Task<IEnumerable<TimeSlot>> GetTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        if (startDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Start date cannot be in the past");

        return await _timeSlotRepository.GetByDateRangeAsync(startDate.Date, endDate.Date);
    }

    public async Task<IEnumerable<TimeSlot>> GetTimeSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        if (startDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Start date cannot be in the past");

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.GetByDoctorAndDateRangeAsync(doctorId, startDate.Date, endDate.Date);
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (date.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Date cannot be in the past");

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        // Generate available time slots dynamically
        return await GenerateAvailableTimeSlotsAsync(doctorId, date);
    }

    /// <summary>
    /// Generates available time slots for a doctor on a given date,
    /// excluding slots that already have appointments.
    /// </summary>
    private async Task<IEnumerable<TimeSlot>> GenerateAvailableTimeSlotsAsync(int doctorId, DateTime date)
    {
        var slots = new List<TimeSlot>();
        var startHour = 9; // 9 AM
        var endHour = 17; // 5 PM
        var slotDuration = 30; // 30 minutes

        // Get existing appointments for this doctor on this date
        var existingAppointments = await _appointmentRepository.GetByDoctorAndDateRangeAsync(
            doctorId, date.Date, date.Date.AddDays(1));
        
        var bookedTimes = existingAppointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentDate.Date.Add(a.AppointmentTime.TimeOfDay))
            .ToHashSet();

        var slotId = 1;
        var now = DateTime.UtcNow;

        for (var hour = startHour; hour < endHour; hour++)
        {
            for (var minute = 0; minute < 60; minute += slotDuration)
            {
                var startTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Utc);
                var endTime = startTime.AddMinutes(slotDuration);

                // Skip if slot is in the past
                if (startTime <= now)
                    continue;

                // Skip if slot is already booked
                if (bookedTimes.Contains(startTime))
                    continue;

                slots.Add(new TimeSlot
                {
                    Id = slotId++,
                    DoctorId = doctorId,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsAvailable = true,
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return slots;
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        if (startDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Start date cannot be in the past");

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.GetAvailableSlotsByDoctorAndDateRangeAsync(doctorId, startDate.Date, endDate.Date);
    }

    public async Task<TimeSlot> CreateTimeSlotAsync(int doctorId, DateTime startTime, DateTime endTime)
    {
        ValidateTimeSlot(doctorId, startTime, endTime);

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        // Check for conflicts
        var hasConflict = await _timeSlotRepository.HasConflictAsync(doctorId, startTime, endTime);
        if (hasConflict)
            throw new InvalidOperationException("This time slot conflicts with an existing slot for the doctor");

        var timeSlot = new TimeSlot
        {
            DoctorId = doctorId,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = true,
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };

        return await _timeSlotRepository.AddAsync(timeSlot);
    }

    public async Task<IEnumerable<TimeSlot>> CreateBulkTimeSlotsAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int slotDurationInMinutes)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (date.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Date cannot be in the past");

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        if (slotDurationInMinutes <= 0 || slotDurationInMinutes > 480)
            throw new ArgumentException("Slot duration must be between 1 and 480 minutes", nameof(slotDurationInMinutes));

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        var slots = new List<TimeSlot>();
        var currentTime = date.Add(startTime);
        var endDateTime = date.Add(endTime);

        while (currentTime.Add(TimeSpan.FromMinutes(slotDurationInMinutes)) <= endDateTime)
        {
            var slotEndTime = currentTime.AddMinutes(slotDurationInMinutes);
            
            // Check for conflicts
            var hasConflict = await _timeSlotRepository.HasConflictAsync(doctorId, currentTime, slotEndTime);
            if (!hasConflict)
            {
                slots.Add(new TimeSlot
                {
                    DoctorId = doctorId,
                    StartTime = currentTime,
                    EndTime = slotEndTime,
                    IsAvailable = true,
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            currentTime = slotEndTime;
        }

        if (slots.Count == 0)
            throw new InvalidOperationException("No valid time slots could be created for the specified date and time range");

        return await _timeSlotRepository.AddBulkAsync(slots);
    }


    public async Task<TimeSlot> UpdateTimeSlotAsync(int id, DateTime startTime, DateTime endTime, bool isAvailable, bool isBlocked, string? blockReason)
    {
        if (id <= 0)
            throw new ArgumentException("TimeSlot ID must be greater than 0", nameof(id));

        ValidateTimeSlot(0, startTime, endTime, true);

        var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
        if (timeSlot == null)
            throw new InvalidOperationException($"TimeSlot with ID {id} not found");

        // Check for conflicts (excluding current slot)
        var hasConflict = await _timeSlotRepository.HasConflictAsync(timeSlot.DoctorId, startTime, endTime, id);
        if (hasConflict)
            throw new InvalidOperationException("This time slot conflicts with another existing slot");

        timeSlot.StartTime = startTime;
        timeSlot.EndTime = endTime;
        timeSlot.IsAvailable = isAvailable;
        timeSlot.IsBlocked = isBlocked;
        timeSlot.BlockReason = blockReason?.Trim();
        timeSlot.UpdatedAt = DateTime.UtcNow;

        return await _timeSlotRepository.UpdateAsync(timeSlot);
    }

    public async Task<TimeSlot> BlockTimeSlotAsync(int id, string? blockReason)
    {
        if (id <= 0)
            throw new ArgumentException("TimeSlot ID must be greater than 0", nameof(id));

        var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
        if (timeSlot == null)
            throw new InvalidOperationException($"TimeSlot with ID {id} not found");

        timeSlot.IsBlocked = true;
        timeSlot.IsAvailable = false;
        timeSlot.BlockReason = blockReason?.Trim();
        timeSlot.UpdatedAt = DateTime.UtcNow;

        return await _timeSlotRepository.UpdateAsync(timeSlot);
    }

    public async Task<TimeSlot> UnblockTimeSlotAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("TimeSlot ID must be greater than 0", nameof(id));

        var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
        if (timeSlot == null)
            throw new InvalidOperationException($"TimeSlot with ID {id} not found");

        timeSlot.IsBlocked = false;
        timeSlot.IsAvailable = true;
        timeSlot.BlockReason = null;
        timeSlot.UpdatedAt = DateTime.UtcNow;

        return await _timeSlotRepository.UpdateAsync(timeSlot);
    }

    public async Task<bool> DeleteTimeSlotAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("TimeSlot ID must be greater than 0", nameof(id));

        return await _timeSlotRepository.DeleteAsync(id);
    }

    public async Task<int> DeleteTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _timeSlotRepository.DeleteByDoctorAndDateAsync(doctorId, date.Date);
    }

    private static void ValidateTimeSlot(int doctorId, DateTime startTime, DateTime endTime, bool allowPast = false)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        if (!allowPast && startTime < DateTime.UtcNow)
            throw new ArgumentException("Start time cannot be in the past");

        var duration = endTime - startTime;
        if (duration.TotalMinutes < 15)
            throw new ArgumentException("Time slot must be at least 15 minutes");

        if (duration.TotalHours > 8)
            throw new ArgumentException("Time slot cannot exceed 8 hours");
    }
}
