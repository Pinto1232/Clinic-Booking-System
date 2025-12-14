using ClinicBookingSystem.Frontend.Models;

namespace ClinicBookingSystem.Frontend.Services;

public class BookingService
{
    private readonly HttpClient _httpClient;
    
    // In-memory storage for demo (replace with API calls later)
    private static List<DoctorModel> _doctors = new()
    {
        new DoctorModel { Id = 1, FirstName = "John", LastName = "Smith", Specialization = "General Medicine", Email = "john.smith@clinic.com", IsAvailable = true },
        new DoctorModel { Id = 2, FirstName = "Sarah", LastName = "Johnson", Specialization = "Pediatrics", Email = "sarah.johnson@clinic.com", IsAvailable = true },
        new DoctorModel { Id = 3, FirstName = "Michael", LastName = "Williams", Specialization = "Cardiology", Email = "michael.williams@clinic.com", IsAvailable = true },
        new DoctorModel { Id = 4, FirstName = "Emily", LastName = "Brown", Specialization = "Dermatology", Email = "emily.brown@clinic.com", IsAvailable = true },
        new DoctorModel { Id = 5, FirstName = "David", LastName = "Davis", Specialization = "Orthopedics", Email = "david.davis@clinic.com", IsAvailable = true }
    };

    private static List<AppointmentModel> _appointments = new();
    private static int _nextAppointmentId = 1;

    public BookingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Get all available doctors
    public Task<List<DoctorModel>> GetDoctorsAsync()
    {
        return Task.FromResult(_doctors.Where(d => d.IsAvailable).ToList());
    }

    // Get doctor by ID
    public Task<DoctorModel?> GetDoctorByIdAsync(int doctorId)
    {
        return Task.FromResult(_doctors.FirstOrDefault(d => d.Id == doctorId));
    }

    // Get available time slots for a doctor on a specific date
    public Task<List<TimeSlotModel>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
    {
        var slots = new List<TimeSlotModel>();
        var startHour = 9; // 9 AM
        var endHour = 17; // 5 PM
        var slotDuration = 30; // 30 minutes

        var slotId = 1;
        for (var hour = startHour; hour < endHour; hour++)
        {
            for (var minute = 0; minute < 60; minute += slotDuration)
            {
                var startTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
                var endTime = startTime.AddMinutes(slotDuration);

                // Check if slot is in the past
                if (startTime <= DateTime.Now)
                    continue;

                // Check if slot is already booked
                var isBooked = _appointments.Any(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.AppointmentTime.TimeOfDay == startTime.TimeOfDay &&
                    a.Status != AppointmentStatus.Cancelled);

                if (!isBooked)
                {
                    slots.Add(new TimeSlotModel
                    {
                        Id = slotId++,
                        DoctorId = doctorId,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsAvailable = true,
                        IsBlocked = false
                    });
                }
            }
        }

        return Task.FromResult(slots);
    }

    // Book an appointment
    public Task<(bool Success, string Message, AppointmentModel? Appointment)> BookAppointmentAsync(CreateAppointmentRequest request)
    {
        // Validation: Check if doctor exists
        var doctor = _doctors.FirstOrDefault(d => d.Id == request.DoctorId);
        if (doctor == null)
        {
            return Task.FromResult<(bool, string, AppointmentModel?)>((false, "Doctor not found.", null));
        }

        // Validation: Check if date is not in the past
        var appointmentDateTime = request.AppointmentDate.Date.Add(request.AppointmentTime.TimeOfDay);
        if (appointmentDateTime <= DateTime.Now)
        {
            return Task.FromResult<(bool, string, AppointmentModel?)>((false, "Cannot book appointments in the past.", null));
        }

        // Validation: Check for double booking
        var hasConflict = _appointments.Any(a =>
            a.DoctorId == request.DoctorId &&
            a.AppointmentDate.Date == request.AppointmentDate.Date &&
            a.AppointmentTime.TimeOfDay == request.AppointmentTime.TimeOfDay &&
            a.Status != AppointmentStatus.Cancelled);

        if (hasConflict)
        {
            return Task.FromResult<(bool, string, AppointmentModel?)>((false, "This time slot is already booked. Please select another time.", null));
        }

        // Create the appointment
        var appointment = new AppointmentModel
        {
            Id = _nextAppointmentId++,
            PatientId = 1, // TODO: Get from authenticated user
            DoctorId = request.DoctorId,
            PatientName = "Current User", // TODO: Get from authenticated user
            DoctorName = doctor.FullName,
            DoctorSpecialization = doctor.Specialization,
            AppointmentDate = request.AppointmentDate,
            AppointmentTime = request.AppointmentTime,
            DurationInMinutes = request.DurationInMinutes,
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _appointments.Add(appointment);

        return Task.FromResult<(bool, string, AppointmentModel?)>((true, "Appointment booked successfully!", appointment));
    }

    // Get user's appointments
    public Task<List<AppointmentModel>> GetMyAppointmentsAsync()
    {
        // TODO: Filter by authenticated user's patient ID
        return Task.FromResult(_appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDateTime)
            .ToList());
    }

    // Get upcoming appointments
    public Task<List<AppointmentModel>> GetUpcomingAppointmentsAsync()
    {
        return Task.FromResult(_appointments
            .Where(a => a.AppointmentDateTime > DateTime.Now && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDateTime)
            .ToList());
    }

    // Cancel an appointment
    public Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, string? reason = null)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment == null)
        {
            return Task.FromResult((false, "Appointment not found."));
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Task.FromResult((false, "Appointment is already cancelled."));
        }

        if (appointment.AppointmentDateTime <= DateTime.Now)
        {
            return Task.FromResult((false, "Cannot cancel past appointments."));
        }

        appointment.Status = AppointmentStatus.Cancelled;

        return Task.FromResult((true, "Appointment cancelled successfully."));
    }

    // Confirm an appointment
    public Task<(bool Success, string Message)> ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment == null)
        {
            return Task.FromResult((false, "Appointment not found."));
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return Task.FromResult((false, "Only scheduled appointments can be confirmed."));
        }

        appointment.Status = AppointmentStatus.Confirmed;

        return Task.FromResult((true, "Appointment confirmed successfully."));
    }
}
