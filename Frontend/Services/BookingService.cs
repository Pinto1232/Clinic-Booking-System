using System.Net.Http.Json;
using ClinicBookingSystem.Frontend.Models;

namespace ClinicBookingSystem.Frontend.Services;

public class BookingService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public BookingService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    // Get all available doctors
    public async Task<List<DoctorModel>> GetDoctorsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<DoctorResponse>>("api/doctors/available");
            return response?.Select(d => new DoctorModel
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Email = d.Email,
                PhoneNumber = d.PhoneNumber,
                Specialization = d.Specialization,
                IsAvailable = d.IsAvailable
            }).ToList() ?? new List<DoctorModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching doctors: {ex.Message}");
            return new List<DoctorModel>();
        }
    }

    // Get doctor by ID
    public async Task<DoctorModel?> GetDoctorByIdAsync(int doctorId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DoctorResponse>($"api/doctors/{doctorId}");
            if (response == null) return null;
            
            return new DoctorModel
            {
                Id = response.Id,
                FirstName = response.FirstName,
                LastName = response.LastName,
                Email = response.Email,
                PhoneNumber = response.PhoneNumber,
                Specialization = response.Specialization,
                IsAvailable = response.IsAvailable
            };
        }
        catch
        {
            return null;
        }
    }

    // Get available time slots for a doctor on a specific date
    public async Task<List<TimeSlotModel>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
    {
        try
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<List<TimeSlotResponse>>(
                $"api/timeslots/doctor/{doctorId}/date/{dateStr}/available");
            
            return response?.Select(t => new TimeSlotModel
            {
                Id = t.Id,
                DoctorId = t.DoctorId,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                IsAvailable = t.IsAvailable,
                IsBlocked = t.IsBlocked
            }).ToList() ?? new List<TimeSlotModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching time slots: {ex.Message}");
            // Generate default time slots if API fails
            return GenerateDefaultTimeSlots(doctorId, date);
        }
    }

    // Generate default time slots (fallback)
    private List<TimeSlotModel> GenerateDefaultTimeSlots(int doctorId, DateTime date)
    {
        var slots = new List<TimeSlotModel>();
        var startHour = 9;
        var endHour = 17;
        var slotDuration = 30;

        var slotId = 1;
        for (var hour = startHour; hour < endHour; hour++)
        {
            for (var minute = 0; minute < 60; minute += slotDuration)
            {
                var startTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
                var endTime = startTime.AddMinutes(slotDuration);

                if (startTime <= DateTime.Now)
                    continue;

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

        return slots;
    }

    // Book an appointment
    public async Task<(bool Success, string Message, AppointmentModel? Appointment)> BookAppointmentAsync(CreateAppointmentRequest request)
    {
        try
        {
            var patientId = _authService.PatientId;
            if (!patientId.HasValue)
            {
                return (false, "User is not authenticated or does not have a patient profile.", null);
            }

            var apiRequest = new
            {
                PatientId = patientId.Value,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = request.AppointmentTime,
                DurationInMinutes = request.DurationInMinutes,
                Reason = request.Reason,
                Notes = request.Notes
            };

            var response = await _httpClient.PostAsJsonAsync("api/appointments", apiRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
                if (result != null)
                {
                    var appointment = MapToAppointmentModel(result);
                    return (true, "Appointment booked successfully!", appointment);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "This time slot is already booked. Please select another time.", null);
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Failed to book appointment.", null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error booking appointment: {ex.Message}");
            return (false, $"An error occurred: {ex.Message}", null);
        }

        return (false, "Failed to book appointment.", null);
    }

    // Get user's appointments
    public async Task<List<AppointmentModel>> GetMyAppointmentsAsync()
    {
        try
        {
            var patientId = _authService.PatientId;
            if (!patientId.HasValue)
            {
                return new List<AppointmentModel>();
            }

            var response = await _httpClient.GetFromJsonAsync<List<AppointmentResponse>>(
                $"api/appointments/patient/{patientId.Value}");
            
            return response?.Select(MapToAppointmentModel).ToList() ?? new List<AppointmentModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching appointments: {ex.Message}");
            return new List<AppointmentModel>();
        }
    }

    // Get upcoming appointments
    public async Task<List<AppointmentModel>> GetUpcomingAppointmentsAsync()
    {
        try
        {
            var patientId = _authService.PatientId;
            if (!patientId.HasValue)
            {
                return new List<AppointmentModel>();
            }

            var response = await _httpClient.GetFromJsonAsync<List<AppointmentResponse>>(
                $"api/appointments/upcoming/patient/{patientId.Value}");
            
            return response?.Select(MapToAppointmentModel).ToList() ?? new List<AppointmentModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching upcoming appointments: {ex.Message}");
            return new List<AppointmentModel>();
        }
    }

    // Cancel an appointment
    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, string? reason = null)
    {
        try
        {
            var request = new { CancellationReason = reason };
            var response = await _httpClient.PatchAsJsonAsync($"api/appointments/{appointmentId}/cancel", request);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Appointment cancelled successfully.");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Failed to cancel appointment.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    // Confirm an appointment
    public async Task<(bool Success, string Message)> ConfirmAppointmentAsync(int appointmentId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/appointments/{appointmentId}/confirm", new { });
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Appointment confirmed successfully.");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Failed to confirm appointment.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    // Complete an appointment
    public async Task<(bool Success, string Message)> CompleteAppointmentAsync(int appointmentId)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/appointments/{appointmentId}/complete", null);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Appointment marked as completed.");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Failed to complete appointment.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    // Restore a cancelled appointment
    public async Task<(bool Success, string Message)> RestoreAppointmentAsync(int appointmentId)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/appointments/{appointmentId}/restore", null);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Appointment restored successfully.");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, error?.Message ?? "Failed to restore appointment.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    // Search patients by name or email
    public async Task<List<PatientSearchResult>> SearchPatientsAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<PatientSearchResult>>($"api/patients/search?term={Uri.EscapeDataString(searchTerm)}");
            return response ?? new List<PatientSearchResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching patients: {ex.Message}");
            return new List<PatientSearchResult>();
        }
    }

    private AppointmentModel MapToAppointmentModel(AppointmentResponse response)
    {
        return new AppointmentModel
        {
            Id = response.Id,
            PatientId = response.PatientId,
            DoctorId = response.DoctorId,
            PatientName = response.PatientName,
            DoctorName = response.DoctorName,
            DoctorSpecialization = response.DoctorSpecialization,
            AppointmentDate = response.AppointmentDate,
            AppointmentTime = response.AppointmentTime,
            DurationInMinutes = response.DurationInMinutes,
            Status = (AppointmentStatus)response.Status,
            Reason = response.Reason,
            Notes = response.Notes,
            CreatedAt = response.CreatedAt
        };
    }
}

// Response DTOs for API communication
public class DoctorResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class TimeSlotResponse
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBlocked { get; set; }
}

public class AppointmentResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int DurationInMinutes { get; set; }
    public int Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ErrorResponse
{
    public string? Message { get; set; }
}
