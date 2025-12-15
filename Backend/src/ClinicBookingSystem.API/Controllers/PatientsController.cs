using ClinicBookingSystem.API.DTOs;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientResponse>> GetPatientById(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = $"Patient with ID {id} not found" });

            return Ok(MapToResponse(patient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/profile")]
    public async Task<ActionResult<PatientProfileResponse>> GetPatientProfile(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = $"Patient with ID {id} not found" });

            return Ok(MapToProfileResponse(patient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient profile");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> SearchPatients([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<PatientResponse>());

            var patients = await _patientService.SearchPatientsAsync(term);
            return Ok(patients.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<PatientResponse>> GetPatientByEmail(string email)
    {
        try
        {
            var patient = await _patientService.GetPatientByEmailAsync(email);
            if (patient == null)
                return NotFound(new { message = $"Patient with email {email} not found" });

            return Ok(MapToResponse(patient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient by email");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAllPatients()
    {
        try
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all patients");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<PatientResponse>> RegisterPatient([FromBody] CreatePatientRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _patientService.RegisterPatientAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber);

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, MapToResponse(patient));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Patient registration failed");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering patient");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PatientResponse>> UpdatePatient(int id, [FromBody] UpdatePatientRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _patientService.UpdatePatientAsync(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber);

            return Ok(MapToResponse(patient));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Patient not found for update");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePatient(int id)
    {
        try
        {
            var success = await _patientService.DeletePatientAsync(id);
            if (!success)
                return NotFound(new { message = $"Patient with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/profile")]
    public async Task<ActionResult<PatientProfileResponse>> UpdatePatientProfile(int id, [FromBody] UpdatePatientProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _patientService.UpdatePatientProfileAsync(
                id,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.DateOfBirth,
                (Gender)(int)request.Gender,
                request.Address,
                request.City,
                request.State,
                request.ZipCode,
                request.InsuranceProvider,
                request.InsurancePolicyNumber,
                request.EmergencyContactName,
                request.EmergencyContactPhone,
                request.EmergencyContactRelationship,
                request.BloodType,
                request.Allergies,
                request.MedicalNotes);

            return Ok(MapToProfileResponse(patient));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Patient not found for profile update");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient profile");
            return BadRequest(new { message = ex.Message });
        }
    }

    private static PatientResponse MapToResponse(Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = patient.GetFullName(),
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            CreatedAt = patient.CreatedAt
        };
    }

    private static PatientProfileResponse MapToProfileResponse(Patient patient)
    {
        return new PatientProfileResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = patient.GetFullName(),
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.GetAge(),
            Gender = (GenderDto)(int)patient.Gender,
            Address = patient.Address,
            City = patient.City,
            State = patient.State,
            ZipCode = patient.ZipCode,
            InsuranceProvider = patient.InsuranceProvider,
            InsurancePolicyNumber = patient.InsurancePolicyNumber,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            EmergencyContactRelationship = patient.EmergencyContactRelationship,
            BloodType = patient.BloodType,
            Allergies = patient.Allergies,
            MedicalNotes = patient.MedicalNotes,
            IsProfileComplete = patient.IsProfileComplete,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }
}
