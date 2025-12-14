using ClinicBookingSystem.Domain.Entities;

namespace ClinicBookingSystem.Domain.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User, string? Token, string? RefreshToken, DateTime? Expiration)> LoginAsync(string email, string password);
    Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber);
    Task<(bool Success, string Message, string? Token, string? RefreshToken, DateTime? Expiration)> RefreshTokenAsync(string token, string refreshToken);
    Task<bool> RevokeTokenAsync(string email);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
