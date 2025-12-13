using ClinicBookingSystem.API.DTOs;
using ClinicBookingSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<TimeSlotsController> _logger;

    public TimeSlotsController(ITimeSlotService timeSlotService, ILogger<TimeSlotsController> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeSlotResponse>> GetTimeSlotById(int id)
    {
        try
        {
            var timeSlot = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (timeSlot == null)
                return NotFound(new { message = $"TimeSlot with ID {id} not found" });

            return Ok(MapToResponse(timeSlot));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetAllTimeSlots()
    {
        try
        {
            var timeSlots = await _timeSlotService.GetAllTimeSlotsAsync();
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all time slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlotsByDoctor(int doctorId)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByDoctorAsync(doctorId);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving doctor time slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor time slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}/available")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetAvailableSlotsByDoctor(int doctorId)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetAvailableSlotsByDoctorAsync(doctorId);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving available slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlotsByDate(DateTime date)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByDateAsync(date);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time slots by date");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}/date/{date}")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlotsByDoctorAndDate(int doctorId, DateTime date)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByDoctorAndDateAsync(doctorId, date);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving doctor time slots by date");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor time slots by date");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}/date/{date}/available")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetAvailableSlotsByDoctorAndDate(int doctorId, DateTime date)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving available slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlotsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByDateRangeAsync(startDate, endDate);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time slots by date range");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}/date-range")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlotsByDoctorAndDateRange(
        int doctorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByDoctorAndDateRangeAsync(doctorId, startDate, endDate);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving doctor time slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor time slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId}/date-range/available")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetAvailableSlotsByDoctorAndDateRange(
        int doctorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var timeSlots = await _timeSlotService.GetAvailableSlotsByDoctorAndDateRangeAsync(doctorId, startDate, endDate);
            return Ok(timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when retrieving available slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TimeSlotResponse>> CreateTimeSlot(CreateTimeSlotRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var timeSlot = await _timeSlotService.CreateTimeSlotAsync(
                request.DoctorId,
                request.StartTime,
                request.EndTime
            );

            return CreatedAtAction(nameof(GetTimeSlotById), new { id = timeSlot.Id }, MapToResponse(timeSlot));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating time slot");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> CreateBulkTimeSlots(CreateBulkTimeSlotsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var timeSlots = await _timeSlotService.CreateBulkTimeSlotsAsync(
                request.DoctorId,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.SlotDurationInMinutes
            );

            return CreatedAtAction(nameof(GetAllTimeSlots), timeSlots.Select(MapToResponse));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating bulk time slots");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk time slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TimeSlotResponse>> UpdateTimeSlot(int id, UpdateTimeSlotRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var timeSlot = await _timeSlotService.UpdateTimeSlotAsync(
                id,
                request.StartTime,
                request.EndTime,
                request.IsAvailable,
                request.IsBlocked,
                request.BlockReason
            );

            return Ok(MapToResponse(timeSlot));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating time slot");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/block")]
    public async Task<ActionResult<TimeSlotResponse>> BlockTimeSlot(int id, [FromBody] BlockTimeSlotRequest request)
    {
        try
        {
            var timeSlot = await _timeSlotService.BlockTimeSlotAsync(id, request.BlockReason);
            return Ok(MapToResponse(timeSlot));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when blocking time slot");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/unblock")]
    public async Task<ActionResult<TimeSlotResponse>> UnblockTimeSlot(int id)
    {
        try
        {
            var timeSlot = await _timeSlotService.UnblockTimeSlotAsync(id);
            return Ok(MapToResponse(timeSlot));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when unblocking time slot");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTimeSlot(int id)
    {
        try
        {
            var success = await _timeSlotService.DeleteTimeSlotAsync(id);
            if (!success)
                return NotFound(new { message = $"TimeSlot with ID {id} not found" });

            return Ok(new { message = "TimeSlot deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting time slot");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("doctor/{doctorId}/date/{date}")]
    public async Task<ActionResult> DeleteTimeSlotsByDoctorAndDate(int doctorId, DateTime date)
    {
        try
        {
            var deletedCount = await _timeSlotService.DeleteTimeSlotsByDoctorAndDateAsync(doctorId, date);
            return Ok(new { message = $"Deleted {deletedCount} time slots" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when deleting time slots");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting time slots");
            return BadRequest(new { message = ex.Message });
        }
    }

    private static TimeSlotResponse MapToResponse(Domain.Entities.TimeSlot timeSlot)
    {
        return new TimeSlotResponse
        {
            Id = timeSlot.Id,
            DoctorId = timeSlot.DoctorId,
            DoctorName = timeSlot.Doctor?.GetFullName(),
            DoctorSpecialization = timeSlot.Doctor?.Specialization,
            StartTime = timeSlot.StartTime,
            EndTime = timeSlot.EndTime,
            DurationInMinutes = timeSlot.GetDurationInMinutes(),
            IsAvailable = timeSlot.IsAvailable,
            IsBlocked = timeSlot.IsBlocked,
            BlockReason = timeSlot.BlockReason,
            IsExpired = timeSlot.IsExpired(),
            IsCurrentlyActive = timeSlot.IsCurrentlyActive(),
            CreatedAt = timeSlot.CreatedAt,
            UpdatedAt = timeSlot.UpdatedAt
        };
    }
}
