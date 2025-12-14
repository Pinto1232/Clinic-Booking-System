using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace ClinicBookingSystem.Frontend.Services;

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public UserInfo? User { get; set; }
}

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private bool _isAuthenticated = false;
    private string? _token;
    private UserInfo? _currentUser;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    public bool IsAuthenticated => _isAuthenticated;
    public UserInfo? CurrentUser => _currentUser;
    public string? Token => _token;
    public int? PatientId => _currentUser?.PatientId;

    public async Task<(bool Success, string? Message)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result?.Success == true)
            {
                _isAuthenticated = true;
                _token = result.Token;
                _currentUser = result.User;
                
                // Set the authorization header for future requests
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                
                OnAuthStateChanged?.Invoke();
                return (true, result.Message);
            }
            
            return (false, result?.Message ?? "Login failed");
        }
        catch (Exception ex)
        {
            return (false, $"Login error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Message)> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new 
            { 
                Email = email, 
                Password = password, 
                ConfirmPassword = password,
                FirstName = firstName, 
                LastName = lastName, 
                PhoneNumber = phoneNumber 
            });
            
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result?.Success == true)
            {
                // Auto-login after registration
                _isAuthenticated = true;
                _token = result.Token;
                _currentUser = result.User;
                
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                
                OnAuthStateChanged?.Invoke();
                return (true, result.Message);
            }
            
            return (false, result?.Message ?? "Registration failed");
        }
        catch (Exception ex)
        {
            return (false, $"Registration error: {ex.Message}");
        }
    }

    public void Logout()
    {
        _isAuthenticated = false;
        _token = null;
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        OnAuthStateChanged?.Invoke();
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }
}
