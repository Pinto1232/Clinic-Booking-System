using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetByPatientAndDateRangeAsync(int patientId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetByDoctorAndDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsByPatientAsync(int patientId);
    Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDateTime, int durationInMinutes, int? excludeAppointmentId = null);
    Task<Appointment> AddAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
