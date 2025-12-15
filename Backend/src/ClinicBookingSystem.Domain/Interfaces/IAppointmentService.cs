using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IAppointmentService
{
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByPatientAsync(int patientId);
    Task<Appointment> ScheduleAppointmentAsync(int patientId, int doctorId, DateTime appointmentDate, DateTime appointmentTime, int durationInMinutes, string? reason, string? notes);
    Task<Appointment> UpdateAppointmentAsync(int id, DateTime appointmentDate, DateTime appointmentTime, int durationInMinutes, AppointmentStatus status, string? reason, string? notes);
    Task<Appointment> ConfirmAppointmentAsync(int id);
    Task<Appointment> CompleteAppointmentAsync(int id);
    Task<Appointment> CancelAppointmentAsync(int id, string? cancellationReason);
    Task<Appointment> RestoreAppointmentAsync(int id);
    Task<Appointment> MarkAsNoShowAsync(int id);
    Task<bool> DeleteAppointmentAsync(int id);
}
