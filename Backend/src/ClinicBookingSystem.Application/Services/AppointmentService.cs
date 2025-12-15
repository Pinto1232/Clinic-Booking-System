using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;

namespace ClinicBookingSystem.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Appointment ID must be greater than 0", nameof(id));

        return await _appointmentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(patientId));

        var patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
            throw new InvalidOperationException($"Patient with ID {patientId} not found");

        return await _appointmentRepository.GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        return await _appointmentRepository.GetByDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        return await _appointmentRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
    {
        return await _appointmentRepository.GetUpcomingAppointmentsAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        return await _appointmentRepository.GetUpcomingAppointmentsByDoctorAsync(doctorId);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByPatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(patientId));

        return await _appointmentRepository.GetUpcomingAppointmentsByPatientAsync(patientId);
    }

    public async Task<Appointment> ScheduleAppointmentAsync(int patientId, int doctorId, DateTime appointmentDate, DateTime appointmentTime, int durationInMinutes, string? reason, string? notes)
    {
        // Validate inputs
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than 0", nameof(patientId));

        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than 0", nameof(doctorId));

        if (durationInMinutes <= 0 || durationInMinutes > 480)
            throw new ArgumentException("Duration must be between 1 and 480 minutes", nameof(durationInMinutes));

        // Check if patient exists
        var patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
            throw new InvalidOperationException($"Patient with ID {patientId} not found");

        // Check if doctor exists
        var doctorExists = await _doctorRepository.ExistsAsync(doctorId);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor with ID {doctorId} not found");

        // Check if doctor is available
        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null || !doctor.IsAvailable)
            throw new InvalidOperationException($"Doctor is not available");

        // Check appointment time is in the future
        var appointmentDateTime = appointmentDate.Add(appointmentTime.TimeOfDay);
        if (appointmentDateTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date and time must be in the future");

        // Check for scheduling conflicts
        var hasConflict = await _appointmentRepository.HasConflictAsync(doctorId, appointmentDateTime, durationInMinutes);
        if (hasConflict)
            throw new InvalidOperationException("Doctor has a scheduling conflict at this time");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = appointmentDate.Date,
            AppointmentTime = appointmentTime,
            DurationInMinutes = durationInMinutes,
            Status = AppointmentStatus.Scheduled,
            Reason = reason?.Trim(),
            Notes = notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return await _appointmentRepository.AddAsync(appointment);
    }

    public async Task<Appointment> UpdateAppointmentAsync(int id, DateTime appointmentDate, DateTime appointmentTime, int durationInMinutes, AppointmentStatus status, string? reason, string? notes)
    {
        if (id <= 0)
            throw new ArgumentException("Appointment ID must be greater than 0", nameof(id));

        if (durationInMinutes <= 0 || durationInMinutes > 480)
            throw new ArgumentException("Duration must be between 1 and 480 minutes", nameof(durationInMinutes));

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        // Check if status is being changed to something invalid
        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled appointment");

        // Check appointment time is in the future (if changing date/time)
        var appointmentDateTime = appointmentDate.Add(appointmentTime.TimeOfDay);
        if (appointmentDateTime <= DateTime.UtcNow && status != AppointmentStatus.Completed)
            throw new InvalidOperationException("Appointment date and time must be in the future");

        // Check for scheduling conflicts (excluding current appointment)
        var hasConflict = await _appointmentRepository.HasConflictAsync(appointment.DoctorId, appointmentDateTime, durationInMinutes, id);
        if (hasConflict)
            throw new InvalidOperationException("Doctor has a scheduling conflict at this time");

        appointment.AppointmentDate = appointmentDate.Date;
        appointment.AppointmentTime = appointmentTime;
        appointment.DurationInMinutes = durationInMinutes;
        appointment.Status = status;
        appointment.Reason = reason?.Trim();
        appointment.Notes = notes?.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<Appointment> ConfirmAppointmentAsync(int id)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        if (appointment.Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Only scheduled appointments can be confirmed");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<Appointment> CompleteAppointmentAsync(int id)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointments cannot be completed");

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<Appointment> CancelAppointmentAsync(int id, string? cancellationReason)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = cancellationReason?.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<Appointment> RestoreAppointmentAsync(int id)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        if (appointment.Status != AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Only cancelled appointments can be restored");

        // Check if the appointment date is in the past
        if (appointment.AppointmentDate.Date < DateTime.Today)
            throw new InvalidOperationException("Cannot restore an appointment with a past date");

        appointment.Status = AppointmentStatus.Scheduled;
        appointment.CancelledAt = null;
        appointment.CancellationReason = null;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<Appointment> MarkAsNoShowAsync(int id)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {id} not found");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark a cancelled appointment as no-show");

        appointment.Status = AppointmentStatus.NoShow;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Appointment ID must be greater than 0", nameof(id));

        return await _appointmentRepository.DeleteAsync(id);
    }
}
