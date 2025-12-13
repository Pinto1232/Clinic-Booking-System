using ClinicBookingSystem.API.DTOs;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorResponse>> GetDoctorById(int id)
    {
        try
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = $"Doctor with ID {id} not found" });

            return Ok(MapToResponse(doctor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<DoctorResponse>> GetDoctorByEmail(string email)
    {
        try
        {
            var doctor = await _doctorService.GetDoctorByEmailAsync(email);
            if (doctor == null)
                return NotFound(new { message = $"Doctor with email {email} not found" });

            return Ok(MapToResponse(doctor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor by email");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("license/{licenseNumber}")]
    public async Task<ActionResult<DoctorResponse>> GetDoctorByLicenseNumber(string licenseNumber)
    {
        try
        {
            var doctor = await _doctorService.GetDoctorByLicenseNumberAsync(licenseNumber);
            if (doctor == null)
                return NotFound(new { message = $"Doctor with license number {licenseNumber} not found" });

            return Ok(MapToResponse(doctor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor by license number");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetAllDoctors()
    {
        try
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all doctors");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetDoctorsBySpecialization(string specialization)
    {
        try
        {
            var doctors = await _doctorService.GetDoctorsBySpecializationAsync(specialization);
            return Ok(doctors.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors by specialization");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetAvailableDoctors()
    {
        try
        {
            var doctors = await _doctorService.GetAvailableDoctorsAsync();
            return Ok(doctors.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available doctors");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DoctorResponse>> RegisterDoctor(CreateDoctorRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _doctorService.RegisterDoctorAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Specialization,
                request.LicenseNumber
            );

            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.Id }, MapToResponse(doctor));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when registering doctor");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering doctor");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DoctorResponse>> UpdateDoctor(int id, UpdateDoctorRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _doctorService.UpdateDoctorAsync(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Specialization,
                request.LicenseNumber,
                request.IsAvailable
            );

            return Ok(MapToResponse(doctor));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating doctor");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/availability")]
    public async Task<ActionResult> SetDoctorAvailability(int id, [FromBody] bool isAvailable)
    {
        try
        {
            await _doctorService.SetDoctorAvailabilityAsync(id, isAvailable);
            return Ok(new { message = $"Doctor availability set to {isAvailable}" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when setting doctor availability");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting doctor availability");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDoctor(int id)
    {
        try
        {
            var success = await _doctorService.DeleteDoctorAsync(id);
            if (!success)
                return NotFound(new { message = $"Doctor with ID {id} not found" });

            return Ok(new { message = "Doctor deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor");
            return BadRequest(new { message = ex.Message });
        }
    }

    private static DoctorResponse MapToResponse(Domain.Entities.Doctor doctor)
    {
        return new DoctorResponse
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            FullName = doctor.GetFullName(),
            Email = doctor.Email,
            PhoneNumber = doctor.PhoneNumber,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            IsAvailable = doctor.IsAvailable,
            CreatedAt = doctor.CreatedAt,
            UpdatedAt = doctor.UpdatedAt
        };
    }
}
