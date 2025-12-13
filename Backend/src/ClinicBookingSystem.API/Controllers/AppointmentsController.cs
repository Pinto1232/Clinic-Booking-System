using ClinicBookingSystem.API.DTOs;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentById(int id)
    {
        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound(new { message = $"Appointment with ID {id} not found" });

            return Ok(MapToResponse(appointment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAllAppointments()
    {
        try
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointmentsByPatient(int patientId)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
            return Ok(appointments.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving patient appointments");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointmentsByDoctor(int doctorId)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
            return Ok(appointments.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving doctor appointments");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointmentsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate);
            return Ok(appointments.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments by date range");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUpcomingAppointments()
    {
        try
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync();
            return Ok(appointments.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("upcoming/doctor/{doctorId}")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUpcomingAppointmentsByDoctor(int doctorId)
    {
        try
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsByDoctorAsync(doctorId);
            return Ok(appointments.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor's upcoming appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("upcoming/patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUpcomingAppointmentsByPatient(int patientId)
    {
        try
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsByPatientAsync(patientId);
            return Ok(appointments.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient's upcoming appointments");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> ScheduleAppointment(CreateAppointmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentService.ScheduleAppointmentAsync(
                request.PatientId,
                request.DoctorId,
                request.AppointmentDate,
                request.AppointmentTime,
                request.DurationInMinutes,
                request.Reason,
                request.Notes
            );

            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when scheduling appointment");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(int id, UpdateAppointmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentService.UpdateAppointmentAsync(
                id,
                request.AppointmentDate,
                request.AppointmentTime,
                request.DurationInMinutes,
                (AppointmentStatus)(int)request.Status,
                request.Reason,
                request.Notes
            );

            return Ok(MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating appointment");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/confirm")]
    public async Task<ActionResult<AppointmentResponse>> ConfirmAppointment(int id)
    {
        try
        {
            var appointment = await _appointmentService.ConfirmAppointmentAsync(id);
            return Ok(MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when confirming appointment");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<AppointmentResponse>> CompleteAppointment(int id)
    {
        try
        {
            var appointment = await _appointmentService.CompleteAppointmentAsync(id);
            return Ok(MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when completing appointment");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/cancel")]
    public async Task<ActionResult<AppointmentResponse>> CancelAppointment(int id, [FromBody] CancelAppointmentRequest request)
    {
        try
        {
            var appointment = await _appointmentService.CancelAppointmentAsync(id, request.CancellationReason);
            return Ok(MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when cancelling appointment");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/no-show")]
    public async Task<ActionResult<AppointmentResponse>> MarkAsNoShow(int id)
    {
        try
        {
            var appointment = await _appointmentService.MarkAsNoShowAsync(id);
            return Ok(MapToResponse(appointment));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when marking appointment as no-show");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking appointment as no-show");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAppointment(int id)
    {
        try
        {
            var success = await _appointmentService.DeleteAppointmentAsync(id);
            if (!success)
                return NotFound(new { message = $"Appointment with ID {id} not found" });

            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment");
            return BadRequest(new { message = ex.Message });
        }
    }

    private static AppointmentResponse MapToResponse(Appointment appointment)
    {
        var appointmentDateTime = appointment.GetAppointmentDateTime();
        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            PatientName = appointment.Patient?.GetFullName(),
            DoctorName = appointment.Doctor?.GetFullName(),
            DoctorSpecialization = appointment.Doctor?.Specialization,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            AppointmentDateTime = appointmentDateTime,
            EndTime = appointment.GetEndTime(),
            DurationInMinutes = appointment.DurationInMinutes,
            Status = (AppointmentStatusDto)(int)appointment.Status,
            StatusDisplay = appointment.GetStatusDisplay(),
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt,
            CancelledAt = appointment.CancelledAt,
            CancellationReason = appointment.CancellationReason,
            IsUpcoming = appointment.IsUpcoming(),
            IsPast = appointment.IsPast()
        };
    }
}
