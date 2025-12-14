using Microsoft.AspNetCore.Components;

namespace ClinicBookingSystem.Frontend.Services;

public class AuthService
{
    private readonly NavigationManager _navigationManager;
    private bool _isAuthenticated = false;

    public event Action? OnAuthStateChanged;

    public AuthService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public bool IsAuthenticated => _isAuthenticated;

    public Task<bool> LoginAsync(string email, string password)
    {
        // TODO: Replace with actual API authentication
        // For now, simulate a successful login
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            _isAuthenticated = true;
            OnAuthStateChanged?.Invoke();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber)
    {
        // TODO: Replace with actual API registration
        // For now, simulate a successful registration
        if (!string.IsNullOrEmpty(email) && 
            !string.IsNullOrEmpty(password) && 
            !string.IsNullOrEmpty(firstName) && 
            !string.IsNullOrEmpty(lastName))
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public void Logout()
    {
        _isAuthenticated = false;
        OnAuthStateChanged?.Invoke();
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }
}
