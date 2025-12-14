using ClinicBookingSystem.API.DTOs;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid request data" });

            var (success, message, user, token, refreshToken, expiration) = 
                await _authService.LoginAsync(request.Email, request.Password);

            if (!success)
                return Unauthorized(new AuthResponse { Success = false, Message = message });

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiration = expiration,
                User = user != null ? new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.GetFullName(),
                    Role = user.Role.ToString(),
                    PatientId = user.PatientId,
                    DoctorId = user.DoctorId
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid request data" });

            var (success, message, user) = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber
            );

            if (!success)
                return BadRequest(new AuthResponse { Success = false, Message = message });

            // Auto-login after registration
            var (loginSuccess, loginMessage, _, token, refreshToken, expiration) = 
                await _authService.LoginAsync(request.Email, request.Password);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiration = expiration,
                User = user != null ? new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.GetFullName(),
                    Role = user.Role.ToString(),
                    PatientId = user.PatientId,
                    DoctorId = user.DoctorId
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during registration" });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid request data" });

            var (success, message, token, refreshToken, expiration) = 
                await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);

            if (!success)
                return Unauthorized(new AuthResponse { Success = false, Message = message });

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiration = expiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid user" });

            await _authService.RevokeTokenAsync(email);

            return Ok(new AuthResponse { Success = true, Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during logout" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid request data" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return BadRequest(new AuthResponse { Success = false, Message = "Invalid user" });

            var (success, message) = await _authService.ChangePasswordAsync(
                userId, 
                request.CurrentPassword, 
                request.NewPassword
            );

            if (!success)
                return BadRequest(new AuthResponse { Success = false, Message = message });

            return Ok(new AuthResponse { Success = true, Message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during password change" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid user" });

            return Ok(new UserInfo
            {
                Id = userId,
                Email = email ?? string.Empty,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                FullName = $"{firstName} {lastName}".Trim(),
                Role = role ?? "Patient"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
