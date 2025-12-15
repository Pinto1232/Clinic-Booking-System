using System.Net.Http.Json;
using ClinicBookingSystem.Frontend.Models;

namespace ClinicBookingSystem.Frontend.Services;

public class ProfileService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ProfileService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<PatientProfileModel?> GetPatientProfileAsync()
    {
        try
        {
            var patientId = _authService.PatientId;
            if (!patientId.HasValue)
            {
                Console.WriteLine("User is not authenticated or does not have a patient profile.");
                return null;
            }

            var response = await _httpClient.GetFromJsonAsync<PatientProfileModel>($"api/patients/{patientId.Value}/profile");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching patient profile: {ex.Message}");
            return null;
        }
    }

    public async Task<PatientProfileModel?> GetPatientProfileByIdAsync(int patientId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PatientProfileModel>($"api/patients/{patientId}/profile");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching patient profile: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string Message, PatientProfileModel? Profile)> UpdatePatientProfileAsync(UpdatePatientProfileRequest request)
    {
        try
        {
            var patientId = _authService.PatientId;
            if (!patientId.HasValue)
            {
                return (false, "User is not authenticated or does not have a patient profile.", null);
            }

            var response = await _httpClient.PutAsJsonAsync($"api/patients/{patientId.Value}/profile", request);
            
            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<PatientProfileModel>();
                return (true, "Profile updated successfully!", profile);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Failed to update profile: {errorContent}", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating patient profile: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    public bool IsProfileComplete()
    {
        // Quick check based on auth state - for full check, call GetPatientProfileAsync
        return _authService.IsAuthenticated && _authService.PatientId.HasValue;
    }
}
