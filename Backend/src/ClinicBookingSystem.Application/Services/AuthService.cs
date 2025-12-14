using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBookingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IPatientRepository patientRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, User? User, string? Token, string? RefreshToken, DateTime? Expiration)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null)
            return (false, "Invalid email or password", null, null, null, null);

        if (!user.IsActive)
            return (false, "Account is deactivated. Please contact support.", null, null, null, null);

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid email or password", null, null, null, null);

        // Generate tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return (true, "Login successful", user, token, refreshToken, expiration);
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(
        string email, string password, string firstName, string lastName, string? phoneNumber)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email))
            return (false, "Email is already registered", null);

        // Create a Patient record for the user
        var patient = new Patient
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLower().Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _patientRepository.AddAsync(patient);

        // Create new user with linked Patient
        var user = new User
        {
            Email = email.ToLower().Trim(),
            PasswordHash = HashPassword(password),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Role = UserRole.Patient,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PatientId = patient.Id,
            Patient = patient
        };

        await _userRepository.AddAsync(user);

        return (true, "Registration successful", user);
    }

    public async Task<(bool Success, string Message, string? Token, string? RefreshToken, DateTime? Expiration)> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
            return (false, "Invalid token", null, null, null);

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return (false, "Invalid token", null, null, null);

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return (false, "Invalid or expired refresh token", null, null, null);

        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());
        await _userRepository.UpdateAsync(user);

        return (true, "Token refreshed successfully", newToken, newRefreshToken, expiration);
    }

    public async Task<bool> RevokeTokenAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        return (true, "Password changed successfully");
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        
        var passwordWithSalt = password + salt;
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
        var hash = Convert.ToBase64String(hashBytes);
        
        return $"{salt}:{hash}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = parts[0];
        var storedHash = parts[1];

        using var sha256 = SHA256.Create();
        var passwordWithSalt = password + salt;
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
        var computedHash = Convert.ToBase64String(hashBytes);

        return storedHash == computedHash;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ClinicBookingSystem";
        var audience = jwtSettings["Audience"] ?? "ClinicBookingSystemUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Name, user.GetFullName()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("userId", user.Id.ToString())
        };

        if (user.PatientId.HasValue)
            claims.Add(new Claim("patientId", user.PatientId.Value.ToString()));

        if (user.DoctorId.HasValue)
            claims.Add(new Claim("doctorId", user.DoctorId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Allow expired tokens for refresh
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "ClinicBookingSystem",
            ValidAudience = jwtSettings["Audience"] ?? "ClinicBookingSystemUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private int GetTokenExpirationMinutes()
    {
        var expiration = _configuration.GetSection("JwtSettings")["TokenExpirationMinutes"];
        return int.TryParse(expiration, out var minutes) ? minutes : 60;
    }

    private int GetRefreshTokenExpirationDays()
    {
        var expiration = _configuration.GetSection("JwtSettings")["RefreshTokenExpirationDays"];
        return int.TryParse(expiration, out var days) ? days : 7;
    }
}
